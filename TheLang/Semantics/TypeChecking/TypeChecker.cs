using System;
using System.Collections.Generic;
using System.Linq;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;
using TheLang.Semantics.TypeChecking.Types;
using Kind = TheLang.Semantics.TypeChecking.Types.TypeInfo.Kind;

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
            node.Type = GetTypeInfo(new TypeInfo(Kind.String));
            return true;
        }

        protected override bool Visit(NeedsToBeInfered node) => true;

        protected override bool Visit(IntegerLiteral node)
        {
            node.Type = GetTypeInfo(new TypeInfo(Kind.Integer));
            return true;
        }

        protected override bool Visit(FloatLiteral node)
        {
            node.Type = GetTypeInfo(new TypeInfo(Kind.Float));
            return true;
        }

        protected override bool Visit(ArrayPostfix node)
        {
            if (!Visit(node.Child))
                return false;

            node.Type = GetTypeInfo(new TypeInfo(Kind.Array, node.Dimensions, node.Child.Type));
            return false;
        }

        protected override bool Visit(TupleLiteral node)
        {
            if (!VisitCollection(node.Items))
                return false;

            node.Type = GetTypeInfo(new TypeInfo(Kind.Tuple, children: node.Items.Select(i => i.Type)));
            return true;
        }

        protected override bool Visit(Declaration node)
        {
            if (!Visit(node.DeclaredType))
                return false;

            var declaredType = node.DeclaredType.Type;
            if (declaredType.Id != Kind.Type)
            {
                _compiler.ReportError(node.DeclaredType.Position, 
                    $"A variable can only be declared a Type, and not {node.DeclaredType.Type}.");
                return false;
            }

            node.Type = declaredType;

            var peekTable = _symbolTable.Peek();
            if (peekTable.ContainsKey(node.Name))
            {
                _compiler.ReportError(node.Position, 
                    $"Variable have already been declared in this scope.");
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

            var declaredType = node.DeclaredType.Type;
            // If declarations type is null, then the type needs to be infered
            if (declaredType == null)
            {
                node.Type = node.Value.Type;
            }
            else if (declaredType.Id != Kind.Type)
            {
                _compiler.ReportError(node.DeclaredType.Position,
                    $"A variable can only be declared a Type, and not {node.DeclaredType.Type}.");
                return false;
            }
            else
            {
                var valueType = node.Value.Type;

                if (!ExpectType(declaredType, valueType))
                {
                    _compiler.ReportError(node.Position, 
                        $"{valueType} is not assignable to {declaredType}.");
                    return false;
                }

                switch (node.Type.Id)
                {
                    case Kind.Integer:
                        if (node.Type.Size == TypeInfo.NeedToBeInferedSize)
                            node.Type = GetTypeInfo(new TypeInfo(Kind.Integer, TypeInfo.Bit64));
                        break;
                    case Kind.Float:
                        if (node.Type.Size == TypeInfo.NeedToBeInferedSize)
                            node.Type = GetTypeInfo(new TypeInfo(Kind.Float, TypeInfo.Bit64));
                        break;

                    default:
                        node.Type = declaredType;
                        break;
                }
            }

            var peekTable = _symbolTable.Peek();
            if (peekTable.ContainsKey(node.Name))
            {
                _compiler.ReportError(node.Position,
                    $"Variable have already been declared in this scope.");
                return false;
            }

            peekTable.Add(node.Name, node.Type);
            return true;
        }

        protected override bool Visit(BinaryOperator node)
        {
            if (!Visit(node.Left))
                return false;

            var leftType = node.Left.Type;
            if (node.Kind == BinaryOperatorKind.Dot)
            {
                // The parser should ensure this, so if we crash, the parser made a mistake
                var name = ((Symbol)node.Right).Name;

                switch (leftType.Id)
                {
                    case Kind.Array:
                        switch (name)
                        {
                            case "length":
                                node.Type = GetTypeInfo(new TypeInfo(Kind.Integer, TypeInfo.Bit64));
                                return true;
                            case "data":
                                node.Type = GetTypeInfo(new TypeInfo(Kind.Pointer, children: leftType.Children));
                                return true;
                            default:
                                _compiler.ReportError(node.Position,
                                    $"Array does not contain field \"{name}\"");
                                return false;
                        }

                    case Kind.String:
                        switch (name)
                        {
                            case "length":
                                node.Type = GetTypeInfo(new IntegerTypeInfo(64, true));
                                return true;
                            case "data":
                                node.Type = GetTypeInfo(new PointerTypeInfo(GetTypeInfo(new IntegerTypeInfo(8, false)), PointerKind.Weak));
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
                                $"{c} does not contain the field \"{name}\"");
                            return false;
                        }

                        node.Type = field.Type;
                        return true;

                    default:
                        _compiler.ReportError(node.Position,
                            $"{node.Left.Type} does not contain the field \"{name}\"");
                        return false;
                }
            }

            if (!Visit(node.Right))
                return false;

            var rightType = node.Right.Type;
            switch (node.Kind)
            {
                case BinaryOperatorKind.As:
                    _compiler.ReportError(node.Position,
                        $"To be implemented");
                    return false;


                case BinaryOperatorKind.PlusAssign:
                case BinaryOperatorKind.MinusAssign:
                case BinaryOperatorKind.TimesAssign:
                case BinaryOperatorKind.DivideAssign:
                case BinaryOperatorKind.ModulusAssign:
                {
                    if (!(node.Left is Symbol) && !(node.Left is BinaryOperator b && b.Kind == BinaryOperatorKind.Dot))
                    {
                        _compiler.ReportError(node.Left.Position, "Left side of assignment is not assignable.");
                        return false;
                    }

                    if (!ExpectType(leftType, rightType))
                    {
                        _compiler.ReportError(node.Position,
                            $"{rightType} is not operator assignable to {leftType}.");
                        return false;
                    }

                    node.Type = leftType;
                    return true;
                }

                case BinaryOperatorKind.Times:
                case BinaryOperatorKind.Divide:
                case BinaryOperatorKind.Modulo:
                case BinaryOperatorKind.Plus:
                case BinaryOperatorKind.Minus:
                case BinaryOperatorKind.LessThan:
                case BinaryOperatorKind.LessThanEqual:
                case BinaryOperatorKind.GreaterThan:
                case BinaryOperatorKind.GreaterThanEqual:
                    if (!AreTypeCompatible(leftType, rightType))
                    {
                        _compiler.ReportError(node.Position,
                            $"Left side of {node.Kind} is not an Int or Float.");
                        return false;
                    }

                    if (!(leftType is IntegerTypeInfo) && !(leftType is FloatTypeInfo))
                    {
                        _compiler.ReportError(node.Position,
                            $"Cannot perform this operator on type {leftType}.");
                        return false;
                    }

                    node.Type = leftType.Size > rightType.Size ? leftType : rightType;
                    return true;

                case BinaryOperatorKind.Equal:
                case BinaryOperatorKind.NotEqual:
                {
                    if (!AreTypeCompatible(leftType, rightType))
                    {
                        _compiler.ReportError(node.Position, 
                            $"{leftType} is not equallity compareable to {rightType}.");
                            return false;
                    }

                    node.Type = leftType.Size > rightType.Size ? leftType : rightType;
                    return true;
                }

                case BinaryOperatorKind.And:
                case BinaryOperatorKind.Or:

                    if (!(leftType is BooleanTypeInfo))
                    {
                        _compiler.ReportError(node.Left.Position,
                            $"{leftType} is not of type Bool, and can therefore not be used in a boolean expression.");
                        return false;
                    }

                    if (!(rightType is BooleanTypeInfo))
                    {
                        _compiler.ReportError(node.Right.Position,
                            $"{rightType} is not of type Bool, and can therefore not be used in a boolean expression.");
                        return false;
                    }
                    
                    node.Type = leftType.Size > rightType.Size ? leftType : rightType;
                    return true;

                case BinaryOperatorKind.Assign:
                {
                    if (!(node.Left is Symbol) && !(node.Left is BinaryOperator b && b.Kind == BinaryOperatorKind.Dot))
                    {
                        _compiler.ReportError(node.Left.Position, "Left side of assignment is not assignable.");
                        return false;
                    }
                    
                    if (!ExpectType(leftType, rightType))
                    {
                        _compiler.ReportError(node.Position, 
                            $"{rightType} is not assignable to {leftType}.");
                            return false;
                    }

                    node.Type = leftType;
                    return true;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override bool Visit(UnaryOperator node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(Symbol node)
        {
            if (TryFindSymbolTypeInfo(node.Name, out var result))
            {
                node.Type = result;
                return true;
            }

            // TODO: Figure out a dependency system, where the compiler returns to this point after the symbol have been resolved
            _compiler.ReportError(node.Position, $"");
            return false;
        }

        protected override bool Visit(CompositTypeLiteral node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(Call node)
        {
            if (!Visit(node.Child))
                return false;
            if (!VisitCollection(node.Arguments))
                return false;

            if (!(node.Child.Type is ProcedureTypeInfo procTypeInfo))
            {
                _compiler.ReportError(node.Position, $"Cannot call {node.Type}.");
                return false;
            }
            
            var zip = node.Arguments.Zip(procTypeInfo.ArgumentTypes, (n, t) => (n, t));

            var index = 1;
            foreach (var (argument, expectedType) in zip)
            {
                var actualType = argument.Type;

                if (!ExpectType(expectedType, actualType))
                {
                    _compiler.ReportError(argument.Position, $"Argument {index} was cannot be passed to procedure. Expected {expectedType}, but got {actualType}.");
                    return false;
                }

                index++;
            }

            node.Type = procTypeInfo.ReturnType;
            return true;
        }

        protected override bool Visit(ProcedureTypeNode node)
        {
            if (!Visit(node.Return))
                return false;
            if (!VisitCollection(node.Arguments))
                return false;

            node.Type = new ProcedureTypeInfo(node.IsFunction, node.Return.Type, node.Arguments.Select(n => n.Type));
            return true;
        }

        private static bool AreTypeCompatible(TypeInfo type1, TypeInfo type2)
        {
            if (type1 is FloatTypeInfo && type2 is FloatTypeInfo)
                return true;

            if (type1 is IntegerTypeInfo itype1 && type2 is IntegerTypeInfo itype2)
                return itype1.IsSigned == itype2.IsSigned;

            return type1.Equals(type2);
        }

        private static bool ExpectType(TypeInfo expected, TypeInfo actual)
        {
            if (expected is FloatTypeInfo && actual is FloatTypeInfo)
                return expected.Size >= actual.Size;

            if (expected is IntegerTypeInfo itype1 && actual is IntegerTypeInfo itype2)
                return itype1.IsSigned == itype2.IsSigned && expected.Size >= actual.Size;

            return expected.Equals(actual);
        }

        private TypeInfo GetTypeInfo(TypeInfo typeInfo)
        {
            if (_existingTypes.TryGetValue(typeInfo, out var result))
                return result;

            _existingTypes.Add(typeInfo, typeInfo);
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