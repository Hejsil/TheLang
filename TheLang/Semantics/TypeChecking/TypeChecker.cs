using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeChecker : Visitor
    {
        private readonly Dictionary<TypeInfoStruct, TypeInfo> _existingTypes =
            new Dictionary<TypeInfoStruct, TypeInfo>(TypeInfoStruct.Comparer);

        private readonly Stack<Dictionary<string, TypeInfo>> _symbolTable =
            new Stack<Dictionary<string, TypeInfo>>();
        private readonly Compiler _compiler;

        public TypeChecker(Compiler compiler)
        {
            _compiler = compiler;
        }

        protected override bool Visit(StringLiteral node)
        {
            var chr = GetTypeInfo(new TypeInfoStruct(TypeId.UInteger, TypeInfo.Bit8));
            var pointer = GetTypeInfo(new TypeInfoStruct(TypeId.Pointer, chr));
            var integer = GetTypeInfo(new TypeInfoStruct(TypeId.Integer, TypeInfo.Bit64));
            var dataField = GetTypeInfo(new TypeInfoStruct(TypeId.Field, "data", pointer));
            var lengthField = GetTypeInfo(new TypeInfoStruct(TypeId.Field, "length", integer));
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.String, dataField, lengthField));
            return true;
        }

        protected override bool Visit(NeedsToBeInfered node) => true;

        protected override bool Visit(IntegerLiteral node)
        {
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Integer));
            return true;
        }

        protected override bool Visit(FloatLiteral node)
        {
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Float));
            return true;
        }

        protected override bool Visit(ArrayPostfix node)
        {
            if (!Visit(node.Child))
                return false;

            var pointer = GetTypeInfo(new TypeInfoStruct(TypeId.Pointer, node.Child.Type));
            var integer = GetTypeInfo(new TypeInfoStruct(TypeId.Integer, TypeInfo.Bit64));
            var dataField = GetTypeInfo(new TypeInfoStruct(TypeId.Field, "data", pointer));
            var lengthField = GetTypeInfo(new TypeInfoStruct(TypeId.Field, "length", integer));
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Array, node.Dimensions, dataField, lengthField));
            return true;
        }

        protected override bool Visit(TupleLiteral node)
        {
            if (!VisitCollection(node.Items))
                return false;

            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Tuple, children: node.Items.Select(i => i.Type)));
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
            else if (declaredType.Id != TypeId.Type)
            {
                _compiler.ReportError(node.DeclaredType.Position,
                    $"A variable can only be declared a Type, and not {node.DeclaredType.Type}.");
                return false;
            }
            else
            {
                var valueType = node.Value.Type;

                if (!valueType.IsImplicitlyConvertibleTo(declaredType))
                {
                    _compiler.ReportError(node.Position,
                        $"{valueType} is not assignable to {declaredType}.");
                    return false;
                }

                switch (node.Type.Id)
                {
                    case TypeId.UInteger:
                    case TypeId.Integer:
                    case TypeId.Float:
                        if (node.Type.Size == TypeInfo.NeedToBeInferedSize)
                            node.Type = GetTypeInfo(new TypeInfoStruct(node.Type.Id, TypeInfo.Bit64));
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

        protected override bool Visit(Declaration node)
        {
            if (!Visit(node.DeclaredType))
                return false;

            var declaredType = node.DeclaredType.Type;
            if (declaredType.Id != TypeId.Type)
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
                    case TypeId.Array:
                    case TypeId.String:
                    case TypeId.Struct:
                        var field = leftType.Children.FirstOrDefault(f => f.Id == TypeId.Field && f.Name == name);

                        if (field == null)
                        {
                            _compiler.ReportError(node.Position,
                                $"{leftType} does not contain the field \"{name}\"");
                            return false;
                        }

                        node.Type = field.Children.First();
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

                    if (!rightType.IsImplicitlyConvertibleTo(leftType))
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
                    if (leftType.IsImplicitlyConvertibleTo(rightType))
                        node.Type = rightType;
                    else if (rightType.IsImplicitlyConvertibleTo(leftType))
                        node.Type = leftType;
                    else
                    {
                        _compiler.ReportError(node.Position,
                            $"Cannot apply {node.Kind} to {leftType} and {rightType}.");
                        return false;
                    }

                    switch (node.Type.Id)
                    {
                        case TypeId.Integer:
                        case TypeId.UInteger:
                        case TypeId.Float:
                            return true;

                        default:
                            _compiler.ReportError(node.Position,
                                $"Cannot apply {node.Kind} to {leftType} and {rightType}.");
                            return false;
                    }

                case BinaryOperatorKind.Equal:
                case BinaryOperatorKind.NotEqual:
                    if (rightType.IsImplicitlyConvertibleTo(leftType))
                        node.Type = rightType;
                    else if (rightType.IsImplicitlyConvertibleTo(leftType))
                        node.Type = leftType;
                    else
                    {
                        _compiler.ReportError(node.Position,
                            $"Cannot apply {node.Kind} to {leftType} and {rightType}.");
                        return false;
                    }

                    return true;

                case BinaryOperatorKind.And:
                case BinaryOperatorKind.Or:
                    if (rightType.IsImplicitlyConvertibleTo(leftType))
                        node.Type = rightType;
                    else if (rightType.IsImplicitlyConvertibleTo(leftType))
                        node.Type = leftType;
                    else
                    {
                        _compiler.ReportError(node.Position,
                            $"Cannot apply {node.Kind} to {leftType} and {rightType}.");
                        return false;
                    }

                    if (node.Type.Id == TypeId.Bool)
                        return true;

                    _compiler.ReportError(node.Position,
                        $"Cannot apply boolean operator to {leftType} and {rightType}.");
                    return false;

                case BinaryOperatorKind.Assign:
                {
                    if (!(node.Left is Symbol) && !(node.Left is BinaryOperator b && b.Kind == BinaryOperatorKind.Dot))
                    {
                        _compiler.ReportError(node.Left.Position, "Left side of assignment is not assignable.");
                        return false;
                    }

                    if (rightType.IsImplicitlyConvertibleTo(leftType))
                    {
                        node.Type = leftType;
                        return true;
                    }

                    _compiler.ReportError(node.Position,
                        $"{rightType} is not assignable to {leftType}.");
                        return false;

                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override bool Visit(UnaryOperator node)
        {
            if (!Visit(node.Child))
                return false;

            var childType = node.Child.Type;
            switch (node.Kind)
            {
                case UnaryOperatorKind.Negative:
                case UnaryOperatorKind.Positive:
                    if (childType.Id != TypeId.Integer &&
                        childType.Id != TypeId.UInteger &&
                        childType.Id != TypeId.Float)
                    {
                        // TODO: Error
                        _compiler.ReportError(node.Position, $"");
                        return false;
                    }

                    node.Type = childType;
                    return true;

                case UnaryOperatorKind.Not:
                    if (childType.Id != TypeId.Bool)
                    {
                        // TODO: Error
                        _compiler.ReportError(node.Position, $"");
                        return false;
                    }

                    node.Type = childType;
                    return true;

                case UnaryOperatorKind.Reference:
                    node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Pointer, childType));
                    return true;

                case UnaryOperatorKind.UniqueReference:
                    node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.UniquePointer, childType));
                    return true;

                case UnaryOperatorKind.Dereference:
                    if (childType.Id != TypeId.Pointer && childType.Id != TypeId.UniquePointer)
                    {
                        // TODO: Error
                        _compiler.ReportError(node.Position, $"");
                        return false;
                    }

                    node.Type = childType.Children.First();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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

        protected override bool Visit(StructLiteral node)
        {
            if (!Visit(node.Child))
                return false;
            if (!node.Values.All(a => Visit(a.Right)))
                return false;

            var childType = node.Child.Type;
            if (childType.Id != TypeId.Type)
            {
                // TODO: Error
                _compiler.ReportError(node.Position, $"");
                return false;
            }

            var structType = childType.Children.First();
            if (structType.Id != TypeId.Struct)
            {
                // TODO: Error
                _compiler.ReportError(node.Position, $"");
                return false;
            }

            var groups = node.Values
                .GroupBy(b => b.Left.Name)
                .SkipWhile(g => g.Count() == 1);

            var duplicates = groups as IGrouping<string, StructLiteral.Assignment>[] ?? groups.ToArray();
            if (duplicates.Length != 0)
            {
                foreach (var group in duplicates)
                {
                    var first = group.First();
                    foreach (var value in group.Skip(1))
                    {
                        // TODO: Error
                        _compiler.ReportError(node.Position,
                            $"");
                    }
                }

                return false;
            }

            foreach (var value in node.Values)
            {
                var symbol = value.Left;
                var expr = value.Right;

                var structField = structType.Children.FirstOrDefault(f => f.Name == symbol.Name);
                if (structField == null)
                {
                    // TODO: Error
                    _compiler.ReportError(value.Position,
                        $"");
                    return false;
                }

                var fieldType = structField.Children.First();
                if (!expr.Type.IsImplicitlyConvertibleTo(fieldType))
                {
                    // TODO: Error
                    _compiler.ReportError(value.Position,
                        $"");
                    return false;
                }
            }

            node.Type = structType;
            return true;
        }

        protected override bool Visit(StructType node)
        {
            if (!VisitCollection(node.Fields))
                return false;

            var fields = node.Fields
                .Where(f => !(f is Variable && ((Variable)f).IsConstant))
                .Select(f => GetTypeInfo(new TypeInfoStruct(TypeId.Field, f.Name, f.Type)));

            var composit = GetTypeInfo(new TypeInfoStruct(TypeId.Struct, children: fields));
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Type, composit));
            return true;
        }

        protected override bool Visit(Call node)
        {
            if (!Visit(node.Child))
                return false;
            if (!VisitCollection(node.Arguments))
                return false;

            var childType = node.Child.Type;
            if (childType.Id != TypeId.Procedure && childType.Id != TypeId.Function)
            {
                _compiler.ReportError(node.Position, $"Cannot call {node.Type}.");
                return false;
            }

            var actualArgCount = node.Arguments.Count();
            var expectedArgCount = childType.Children.Count() - 1;
            if (actualArgCount != expectedArgCount)
            {
                _compiler.ReportError(node.Position,
                    $"Procedure takes {expectedArgCount} arguments, but was provide {actualArgCount}.");
                return false;
            }

            var zip = node.Arguments.Zip(childType.Children, (n, t) => (n, t));

            var index = 1;
            foreach (var (argument, expectedType) in zip)
            {
                var actualType = argument.Type;

                if (!actualType.IsImplicitlyConvertibleTo(expectedType))
                {
                    _compiler.ReportError(argument.Position, $"Argument {index} was cannot be passed to procedure. Expected {expectedType}, but got {actualType}.");
                    return false;
                }

                Debug.Assert(actualArgCount >= index);
                index++;
            }

            node.Type = childType.Children.Last();
            return true;
        }

        protected override bool Visit(ProcedureTypeNode node)
        {
            if (!VisitCollection(node.Arguments))
                return false;

            var kind = node.IsFunction ? TypeId.Function : TypeId.Procedure;
            node.Type = GetTypeInfo(new TypeInfoStruct(kind, children: node.Arguments.Select(n => n.Type)));
            return true;
        }

        private TypeInfo GetTypeInfo(TypeInfoStruct typeInfo)
        {
            if (_existingTypes.TryGetValue(typeInfo, out var result))
                return result;

            var instance = typeInfo.Allocate();
            _existingTypes.Add(typeInfo, instance);
            return instance;
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