using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;
using TheLang.Semantics.TypeChecking.Types;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeChecker : Visitor
    {
        private readonly Dictionary<TypeInfo, TypeInfo> _existingTypes = new Dictionary<TypeInfo, TypeInfo>();
        private readonly Stack<Dictionary<string, TypeInfo>> _symbolTable = new Stack<Dictionary<string, TypeInfo>>();
        private readonly Compiler _compiler;

        public TypeChecker(Compiler compiler)
        {
            _compiler = compiler;
        }

        protected override bool Visit(StringLiteral node)
        {
            node.Type = GetTypeInfo(new StringTypeInfo());
            return true;
        }

        protected override bool Visit(NeedsToBeInfered node) => true;

        protected override bool Visit(IntegerLiteral node)
        {
            node.Type = GetTypeInfo(new IntegerTypeInfo(TypeInfo.NeedToBeInferedSize, true));
            return true;
        }

        protected override bool Visit(FloatLiteral node)
        {
            node.Type = GetTypeInfo(new FloatTypeInfo(TypeInfo.NeedToBeInferedSize));
            return true;
        }

        protected override bool Visit(ArrayPostfix node)
        {
            if (!Visit(node.Child))
                return false;

            node.Type = GetTypeInfo(new ArrayTypeInfo(node.Child.Type, node.Dimensions));
            return false;
        }

        protected override bool Visit(TupleLiteral node)
        {
            var elementTypes = new List<TypeInfo>();

            foreach (var item in node.Items)
            {
                if (Visit(item))
                    return false;

                elementTypes.Add(item.Type);
            }

            node.Type = GetTypeInfo(new TupleTypeInfo(elementTypes));
            return true;
        }

        protected override bool Visit(Declaration node)
        {
            if (!Visit(node.DeclaredType))
                return false;

            node.Type = node.DeclaredType.Type;

            var peekTable = _symbolTable.Peek();
            if (peekTable.ContainsKey(node.Name))
            {
                // TODO: Error message
                _compiler.ReportError(node.Position, "");
                return false;
            }

            peekTable.Add(node.Name, node.Type);
            return true;
        }

        protected override bool Visit(Variable node)
        {
            if (!Visit(node.DeclaredType))
                return false;
            if (!Visit(node.Value))
                return false;

            // If declarations type is null, then the type needs to be infered
            if (node.DeclaredType.Type == null)
            {
                node.Type = node.Value.Type;
            }
            else
            {
                var declaredType = node.DeclaredType.Type;
                var valueType = node.Value.Type;

                if (declaredType.GetType() == valueType.GetType() &&
                    (declaredType is IntegerTypeInfo || valueType is FloatTypeInfo))
                {

                }
                else if (declaredType.Equals(valueType))
                {
                }
                else
                {
                    _compiler.ReportError(node.Position, $"");
                    return false;
                }
            }

            switch (node.Type)
            {
                case IntegerTypeInfo i:
                    if (i.Size == TypeInfo.NeedToBeInferedSize)
                        node.Type = GetTypeInfo(new IntegerTypeInfo(TypeInfo.Int64Size, i.IsSigned));
                    break;
                case FloatTypeInfo f:
                    if (f.Size == TypeInfo.NeedToBeInferedSize)
                        node.Type = GetTypeInfo(new FloatTypeInfo(TypeInfo.Int64Size));
                    break;
            }

            var peekTable = _symbolTable.Peek();
            if (peekTable.ContainsKey(node.Name))
            {
                // TODO: Error message
                _compiler.ReportError(node.Position, "");
                return false;
            }

            peekTable.Add(node.Name, node.Type);
            return true;
        }

        protected override bool Visit(BinaryOperator node)
        {
            if (!Visit(node.Left))
                return false;

            if (node.Kind == BinaryOperatorKind.Dot)
            {
                // The parser should ensure this, so if we crash, the parser made a mistake
                var name = ((Symbol)node.Right).Name;

                switch (node.Left.Type)
                {
                    case ArrayTypeInfo a:
                        switch (name)
                        {
                            case "length":
                                node.Type = GetTypeInfo(new IntegerTypeInfo(64, true));
                                return true;
                            case "data":
                                node.Type = GetTypeInfo(new PointerTypeInfo(a.ElementType, PointerKind.Weak));
                                return true;
                            default:
                                _compiler.ReportError(node.Position,
                                    $"Array does not contain field \"{name}\"");
                                return false;
                        }

                    case StringTypeInfo s:
                        switch (name)
                        {
                            case "length":
                                node.Type = GetTypeInfo(new IntegerTypeInfo(64, true));
                                return true;
                            case "data":
                                node.Type = GetTypeInfo(new PointerTypeInfo(GetTypeInfo(new IntegerTypeInfo(8, false)),
                                    PointerKind.Weak));
                                return true;
                            default:
                                _compiler.ReportError(node.Position,
                                    $"String does not contain field \"{name}\"");
                                return false;
                        }

                    case CompositTypeInfo c:
                        var field = c.Fields.FirstOrDefault(f => f.Name == name);

                        if (field == null)
                        {
                            _compiler.ReportError(node.Position,
                                $"? does not contain the field \"{name}\"");
                            return false;
                        }

                        node.Type = field.Type;
                        return true;

                    default:
                        _compiler.ReportError(node.Position,
                            $"Left side of the dot operator does not contain the field \"{name}\"");
                        return false;
                }
            }

            if (!Visit(node.Right))
                return false;

            switch (node.Kind)
            {
                case BinaryOperatorKind.As:
                    _compiler.ReportError(node.Position,
                        $"To be implemented");
                    return false;


                case BinaryOperatorKind.Assign:
                case BinaryOperatorKind.PlusAssign:
                case BinaryOperatorKind.MinusAssign:
                case BinaryOperatorKind.TimesAssign:
                case BinaryOperatorKind.DivideAssign:
                case BinaryOperatorKind.ModulusAssign:
                case BinaryOperatorKind.Times:
                case BinaryOperatorKind.Divide:
                case BinaryOperatorKind.Modulo:
                case BinaryOperatorKind.Plus:
                case BinaryOperatorKind.Minus:
                case BinaryOperatorKind.LessThan:
                case BinaryOperatorKind.LessThanEqual:
                case BinaryOperatorKind.GreaterThan:
                case BinaryOperatorKind.GreaterThanEqual:
                    var leftType = node.Left.Type;
                    var rightType = node.Right.Type;

                    if (!(leftType is IntegerTypeInfo) && !(leftType is FloatTypeInfo))
                    {
                        _compiler.ReportError(node.Position,
                            $"Left side of {node.Kind} is not an Int or Float.");
                        return false;
                    }

                    if (!(rightType is IntegerTypeInfo) && !(rightType is FloatTypeInfo))
                    {
                        _compiler.ReportError(node.Position,
                            $"Right side of {node.Kind} is not an Int or Float.");
                        return false;
                    }

                    if (leftType.GetType() != rightType.GetType())
                    {
                        _compiler.ReportError(node.Position,
                            $"Left and right type are not compatible.");
                        return false;
                    }

                    node.Type = leftType.Size > rightType.Size ? leftType : rightType;
                    return true;

                case BinaryOperatorKind.Equal:
                    break;

                case BinaryOperatorKind.NotEqual:
                    break;

                case BinaryOperatorKind.And:
                    break;

                case BinaryOperatorKind.Or:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        protected override bool Visit(UnaryOperator node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(Symbol node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(CompositTypeLiteral node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(Call node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ProcedureLiteral node)
        {
            throw new NotImplementedException();
        }

        private TypeInfo GetTypeInfo(TypeInfo typeInfo)
        {
            if (_existingTypes.TryGetValue(typeInfo, out var result))
                return result;

            return typeInfo;
        }

        private bool TryFindSymbolTypeInfo(string name, out TypeInfo result)
        {
            foreach (var table in _symbolTable)
            {
                if (table.TryGetValue(name, out result))
                    return true;
            }

            result = null;
            return false;
        }
    }
}