using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Expressions.Operators.Unary;
using TheLang.AST.Statments;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeChecker : Visitor
    {
        private readonly Dictionary<TypeInfoStruct, TypeInfo> _existingTypes =
            new Dictionary<TypeInfoStruct, TypeInfo>(TypeInfoStruct.Comparer);

        private readonly Stack<Dictionary<string, TypeInfo>> _symbolTable = new Stack<Dictionary<string, TypeInfo>>();
        private readonly Stack<TypeInfo> _returnTypeStack = new Stack<TypeInfo>();

        private readonly Compiler _compiler;

        public TypeChecker(Compiler compiler)
        {
            _compiler = compiler;
        }

        protected override bool Visit(Return node)
        {
            if (!Visit(node.Child))
                return false;

            var returnType = _returnTypeStack.Peek();
            var childType = node.Child.Type;
            if (!childType.IsImplicitlyConvertibleTo(returnType))
            {
                _compiler.ReportError(node.Child.Position,
                    $"Cannot return value of type {childType}. Procedure returns {returnType}.");
                return false;
            }

            node.Type = returnType;
            return true;
        }

        protected override bool Visit(TypedProcedureLiteral node)
        {
            _symbolTable.Push(new Dictionary<string, TypeInfo>());

            if (!VisitCollection(node.Arguments) || !Visit(node.Return))
            {
                _symbolTable.Pop();
                return false;
            }

            var args = new List<TypeInfo>(node.Arguments.Select(a => a.Type)) { node.Return.Type };
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Procedure, args));
            _returnTypeStack.Push(node.Return.Type);

            var result = Visit(node.Block);
            _symbolTable.Pop();
            _returnTypeStack.Pop();

            return result;
        }

        protected override bool Visit(ProgramNode node)
        {
            var int64 = GetTypeInfo(new TypeInfoStruct(TypeId.Integer, TypeInfo.Bit64));
            var uint64 = GetTypeInfo(new TypeInfoStruct(TypeId.UInteger, TypeInfo.Bit64));
            var float64 = GetTypeInfo(new TypeInfoStruct(TypeId.Float, TypeInfo.Bit64));
            var bool8 = GetTypeInfo(new TypeInfoStruct(TypeId.Bool, TypeInfo.Bit8));

            var top = new Dictionary<string, TypeInfo>()
            {
                { "Int", GetTypeInfo(new TypeInfoStruct(TypeId.Type, int64)) },
                { "UInt", GetTypeInfo(new TypeInfoStruct(TypeId.Type, uint64)) },
                { "Float", GetTypeInfo(new TypeInfoStruct(TypeId.Type, float64)) },
                { "Bool", GetTypeInfo(new TypeInfoStruct(TypeId.Type, bool8)) },
            };

            _symbolTable.Push(top);
            var result = base.Visit(node);
            _symbolTable.Pop();

            return result;
        }

        protected override bool Visit(StringLiteral node)
        {
            var chr = GetTypeInfo(new TypeInfoStruct(TypeId.UInteger, TypeInfo.Bit8));
            var pointer = GetTypeInfo(new TypeInfoStruct(TypeId.Pointer, chr));
            var integer = GetTypeInfo(new TypeInfoStruct(TypeId.Integer, TypeInfo.Bit64));
            var dataField = GetTypeInfo(new TypeInfoStruct(TypeId.Field, "data", pointer));
            var lengthField = GetTypeInfo(new TypeInfoStruct(TypeId.Field, "length", integer));
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.String, chr, dataField, lengthField));
            return true;
        }

        protected override bool Visit(IntegerLiteral node)
        {
            node.Type = GetTypeInfo(new TypeInfoStruct(node.Value < 0 ? TypeId.Number : TypeId.UNumber));
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
            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Array, node.Child.Type, dataField, lengthField));
            return true;
        }

        protected override bool Visit(TupleLiteral node)
        {
            if (!VisitCollection(node.Items))
                return false;

            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Tuple, node.Items.Select(i => i.Type)));
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

                if (node.Type.Id == TypeId.UNumber || node.Type.Id == TypeId.Number)
                    node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Integer, TypeInfo.Bit64));
            }
            else if (declaredType.Id != TypeId.Type)
            {
                _compiler.ReportError(node.DeclaredType.Position,
                    $"A variable can only be declared a Type, and not {declaredType.Id}.");
                return false;
            }
            else
            {
                declaredType = declaredType.Children.First();
                var valueType = node.Value.Type;

                if (!valueType.IsImplicitlyConvertibleTo(declaredType))
                {
                    _compiler.ReportError(node.Position,
                        $"{valueType.Id} is not assignable to {declaredType.Id}.");
                    return false;
                }

                node.Type = declaredType;
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

        protected override bool Visit(Infer node) => true;

        protected override bool Visit(Assign node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(Declaration node)
        {
            if (!Visit(node.DeclaredType))
                return false;

            var declaredType = node.DeclaredType.Type;
            if (declaredType.Id != TypeId.Type)
            {
                _compiler.ReportError(node.DeclaredType.Position,
                    $"A variable can only be declared a Type, and not {declaredType}.");
                return false;
            }

            node.Type = declaredType.Children.First();

            var peekTable = _symbolTable.Peek();
            if (peekTable.ContainsKey(node.Name))
            {
                _compiler.ReportError(node.Position,
                    "Variable have already been declared in this scope.");
                return false;
            }

            peekTable.Add(node.Name, node.Type);
            return true;
        }


        private bool VisitArithmeticOperators(BinaryNode node)
        {
            if (!Visit(node.Left))
                return false;
            if (!Visit(node.Right))
                return false;

            var leftType = node.Left.Type;
            var rightType = node.Right.Type;

            if (leftType.IsImplicitlyConvertibleTo(rightType))
                node.Type = rightType;
            else if (rightType.IsImplicitlyConvertibleTo(leftType))
                node.Type = leftType;
            else
            {
                _compiler.ReportError(node.Position,
                    $"Cannot apply ?? to {leftType} and {rightType}.");
                return false;
            }

            switch (node.Type.Id)
            {
                case TypeId.UNumber:
                case TypeId.Number:
                case TypeId.Integer:
                case TypeId.UInteger:
                case TypeId.Float:
                    return true;

                default:
                    _compiler.ReportError(node.Position,
                        $"Cannot apply ?? to {leftType} and {rightType}.");
                    return false;
            }
        }

        private bool VisitEqualityOperators(BinaryNode node)
        {
            if (!Visit(node.Left))
                return false;
            if (!Visit(node.Right))
                return false;

            var leftType = node.Left.Type;
            var rightType = node.Right.Type;

            if (!rightType.IsImplicitlyConvertibleTo(leftType) || !leftType.IsImplicitlyConvertibleTo(rightType))
            {
                _compiler.ReportError(node.Position,
                    $"Cannot apply ?? to {leftType} and {rightType}.");
                return false;
            }

            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Bool));
            return true;
        }

        private bool VisitRelationalOperators(BinaryNode node)
        {
            if (!Visit(node.Left))
                return false;
            if (!Visit(node.Right))
                return false;

            var leftType = node.Left.Type;
            var rightType = node.Right.Type;
            TypeInfo compare;

            if (leftType.IsImplicitlyConvertibleTo(rightType))
                compare = rightType;
            else if (rightType.IsImplicitlyConvertibleTo(leftType))
                compare = leftType;
            else
            {
                _compiler.ReportError(node.Position,
                    $"Cannot apply ?? to {leftType} and {rightType}.");
                return false;
            }

            switch (compare.Id)
            {
                case TypeId.UNumber:
                case TypeId.Number:
                case TypeId.Integer:
                case TypeId.UInteger:
                case TypeId.Float:
                    node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Bool));
                    return true;

                default:
                    _compiler.ReportError(node.Position,
                        $"Cannot apply ?? to {leftType} and {rightType}.");
                    return false;
            }
        }

        private bool VisitLogicalOperators(BinaryNode node)
        {
            if (!Visit(node.Left))
                return false;
            if (!Visit(node.Right))
                return false;

            var leftType = node.Left.Type;
            var rightType = node.Right.Type;

            if (rightType.IsImplicitlyConvertibleTo(leftType))
                node.Type = rightType;
            else if (rightType.IsImplicitlyConvertibleTo(leftType))
                node.Type = leftType;
            else
            {
                _compiler.ReportError(node.Position,
                    $"Cannot apply ?? to {leftType} and {rightType}.");
                return false;
            }

            if (node.Type.Id == TypeId.Bool)
                return true;

            _compiler.ReportError(node.Position,
                $"Cannot apply boolean operator to {leftType} and {rightType}.");
            return false;
        }

        protected override bool Visit(UniqueReference node)
        {
            if (!Visit(node.Child))
                return false;

            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.UniquePointer, node.Child.Type));
            return true;
        }

        protected override bool Visit(Symbol node)
        {
            TypeInfo result;
            if (TryFindSymbolTypeInfo(node.Name, out result))
            {
                node.Type = result;
                return true;
            }

            // TODO: Figure out a dependency system, where the compiler returns to this point after the symbol have been resolved
            _compiler.ReportError(node.Position, $"{node.Name} is not declared");
            return false;
        }

        protected override bool Visit(TypeLiteral node)
        {
            if (!Visit(node.Child))
                return false;
            if (!VisitCollection(node.Values))
                return false;



            // TODO: Implement
            _compiler.ReportError(node.Position, $"Not Implemented");
            return false;
        }

        protected override bool Visit(Add node)
        {
            return VisitArithmeticOperators(node);
        }

        protected override bool Visit(And node)
        {
            return VisitLogicalOperators(node);
        }

        protected override bool Visit(As node)
        {
            if (!Visit(node.Left))
                return false;
            if (!Visit(node.Right))
                return false;

            var type = node.Right.Type;
            if (type.Id != TypeId.Type)
            {
                // TODO: Error
                _compiler.ReportError(node.Position, $"Right side of the as operator must be a type, but was {type}");
                return false;
            }

            // TODO: Maybe check if we can even typecast to the type?
            node.Type = type.Children.First();
            return true;
        }

        protected override bool Visit(Divide node)
        {
            return VisitArithmeticOperators(node);
        }

        protected override bool Visit(Dot node)
        {
            if (!Visit(node.Left))
                return false;

            var leftType = node.Left.Type;

            var name = node.Right.Name;
            var field = leftType.Children.FirstOrDefault(f => f.Id == TypeId.Field && f.Name == name);

            if (field == null)
            {
                _compiler.ReportError(node.Position,
                    $"{leftType} does not contain the field \"{name}\"");
                return false;
            }

            node.Type = field.Children.First();
            return true;
        }

        protected override bool Visit(Equal node)
        {
            return VisitEqualityOperators(node);
        }

        protected override bool Visit(GreaterThan node)
        {
            return VisitRelationalOperators(node);
        }

        protected override bool Visit(GreaterThanEqual node)
        {
            return VisitRelationalOperators(node);
        }

        protected override bool Visit(LessThan node)
        {
            return VisitRelationalOperators(node);
        }

        protected override bool Visit(LessThanEqual node)
        {
            return VisitRelationalOperators(node);
        }

        protected override bool Visit(Modulo node)
        {
            return VisitArithmeticOperators(node);
        }

        protected override bool Visit(NotEqual node)
        {
            return VisitEqualityOperators(node);
        }

        protected override bool Visit(Or node)
        {
            return VisitLogicalOperators(node);
        }

        protected override bool Visit(Sub node)
        {
            return VisitArithmeticOperators(node);
        }

        protected override bool Visit(Times node)
        {
            return VisitArithmeticOperators(node);
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

        protected override bool Visit(Dereference node)
        {
            if (!Visit(node.Child))
                return false;

            var childType = node.Child.Type;
            if (childType.Id != TypeId.Pointer && childType.Id != TypeId.UniquePointer)
            {
                _compiler.ReportError(node.Position, $"Cannot dereference {childType}.");
                return false;
            }

            node.Type = childType.Children.First();
            return true;
        }

        protected override bool Visit(Negative node)
        {
            if (!Visit(node.Child))
                return false;

            var childType = node.Child.Type;

            switch (childType.Id)
            {
                case TypeId.Float:
                case TypeId.Number:
                case TypeId.Integer:
                    node.Type = childType;
                    return true;
                case TypeId.UNumber:
                    node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Number));
                    return true;
                default:
                    _compiler.ReportError(node.Position, $"Cannot take the negative value of {childType}.");
                    return false;
            }
        }

        protected override bool Visit(Not node)
        {
            if (!Visit(node.Child))
                return false;

            var childType = node.Child.Type;
            if (childType.Id != TypeId.Bool)
            {
                _compiler.ReportError(node.Position, $"Cannot negate {childType}.");
                return false;
            }

            node.Type = childType;
            return true;
        }

        protected override bool Visit(Positive node)
        {
            if (!Visit(node.Child))
                return false;

            var childType = node.Child.Type;

            switch (childType.Id)
            {
                case TypeId.Float:
                case TypeId.Number:
                case TypeId.UNumber:
                case TypeId.Integer:
                case TypeId.UInteger:
                    break;
                default:
                    _compiler.ReportError(node.Position, $"Cannot apply unary + on {childType}.");
                    return false;
            }

            node.Type = childType;
            return true;
        }

        protected override bool Visit(Reference node)
        {
            if (!Visit(node.Child))
                return false;

            node.Type = GetTypeInfo(new TypeInfoStruct(TypeId.Pointer, node.Child.Type));
            return true;
        }

        protected override bool Visit(Indexing node)
        {
            if (!Visit(node.Child))
                return false;
            if (!Visit(node.Argument))
                return false;

            var childType = node.Child.Type;
            if (childType.Id != TypeId.Array || childType.Id != TypeId.String)
            {
                _compiler.ReportError(node.Position, $"Cannot index {childType}.");
                return false;
            }

            var argumentType = node.Argument.Type;
            if (argumentType.Id != TypeId.Integer && argumentType.Id != TypeId.UInteger)
            {
                // TODO: Error
                _compiler.ReportError(node.Position, $"Indexing does not take {argumentType} as an argument.");
                return false;
            }

            // Array type info have child, which is not a field, that says what the childrens type is, without needing
            // to find the field "data" and go through the pointer for this field
            node.Type = childType.Children.First(field => field.Id != TypeId.Field);
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

            var zip = node.Arguments.Zip(childType.Children, Tuple.Create);

            var index = 1;
            foreach (var tuple in zip)
            {
                var argument = tuple.Item1;
                var expectedType = tuple.Item2;

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

        protected override bool Visit(ProcedureType node)
        {
            if (!VisitCollection(node.Arguments))
                return false;

            var kind = node.IsFunction ? TypeId.Function : TypeId.Procedure;
            node.Type = GetTypeInfo(new TypeInfoStruct(kind, children: node.Arguments.Select(n => n.Type)));
            return true;
        }

        private TypeInfo GetTypeInfo(TypeInfoStruct typeInfo)
        {
            TypeInfo result;
            if (_existingTypes.TryGetValue(typeInfo, out result))
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