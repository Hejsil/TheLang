using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Hosting;
using System.Runtime.Remoting.Messaging;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Expressions.Operators.Unary;
using TheLang.AST.Expressions.Types;
using TheLang.AST.Statments;
using TheLang.Semantics.TypeChecking.Types;
using TheLang.Syntax;
using TheLang.Util;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeChecker : Visitor
    {
        private class TypeCache
        {
            private readonly Dictionary<int, FloatType> _floatCache = new Dictionary<int, FloatType>();
            private readonly Dictionary<(int, bool), IntegerType> _intCache = new Dictionary<(int, bool), IntegerType>();
            private readonly Dictionary<BaseType, ArrayType> _arrayCache = new Dictionary<BaseType, ArrayType>();
            private readonly Dictionary<BaseType, PointerType> _pointerCache = new Dictionary<BaseType, PointerType>();
            private readonly Dictionary<BaseType, TypeType> _typeCache = new Dictionary<BaseType, TypeType>();
            private readonly BooleanType _boolean = new BooleanType();
            private readonly UnknownType _unknown = new UnknownType();
            private readonly VoidType _void = new VoidType();
            private readonly StringType _string = new StringType();

            public ArrayType GetArray(BaseType elementTypes)
            {
                if (_arrayCache.TryGetValue(elementTypes, out var result)) return result;

                result = new ArrayType(elementTypes);
                _arrayCache.Add(elementTypes, result);
                return result;
            }

            public PointerType GetPointer(BaseType elementTypes)
            {
                if (_pointerCache.TryGetValue(elementTypes, out var result)) return result;

                result = new PointerType(elementTypes);
                _pointerCache.Add(elementTypes, result);
                return result;
            }

            public TypeType GetType(BaseType elementTypes)
            {
                if (_typeCache.TryGetValue(elementTypes, out var result)) return result;

                result = new TypeType(elementTypes);
                _typeCache.Add(elementTypes, result);
                return result;
            }

            public FloatType GetFloat(int size)
            {
                if (_floatCache.TryGetValue(size, out var result)) return result;

                result = new FloatType(size);
                _floatCache.Add(size, result);
                return result;
            }

            public IntegerType GetInt(int size, bool signed)
            {
                if (_intCache.TryGetValue((size, signed), out var result)) return result;

                result = new IntegerType(size, signed);
                _intCache.Add((size, signed), result);
                return result;
            }

            public BooleanType GetBoolean() => _boolean;
            public StringType GetString() => _string;
            public UnknownType GetUnknown() => _unknown;
            public VoidType GetVoid() => _void;
        }

        private readonly Compiler _compiler;
        private readonly TypeCache _cache = new TypeCache();
        private readonly Stack<ASTLambda> _procedureStack = new Stack<ASTLambda>();
        private Scope _scope = new Scope();

        public TypeChecker(Compiler compiler)
        {
            _compiler = compiler;

            // Predefined types
            _scope.TryAddSymbol("I64", _cache.GetType(_cache.GetInt(64, true)));
            _scope.TryAddSymbol("F64", _cache.GetType(_cache.GetFloat(64)));
            _scope.TryAddSymbol("String", _cache.GetType(_cache.GetString()));
            _scope.TryAddSymbol("Type", _cache.GetType(_cache.GetType(_cache.GetUnknown())));
        }


        protected override bool Visit(ASTDeclaration node)
        {
            if (!Visit(node.DeclaredType)) return false;

            node.TypeInfo = node.DeclaredType.TypeInfo;

            if (_scope.TryAddSymbol(node.Name, node.TypeInfo)) return true;

            Error(node.Position, "");
            return false;
        }

        protected override bool Visit(ASTReturn node)
        {
            if (!Visit(node.Child)) return false;

            var lambda = _procedureStack.Peek();
            var returnType = ((ProcedureType) lambda.TypeInfo).Return;
            node.TypeInfo = node.Child.TypeInfo;

            return Expect(node.Position, returnType, node.TypeInfo);
        }

        protected override bool Visit(ASTVariable node)
        {
            if (!Visit(node.Value, node.DeclaredType)) return false;

            if (node.DeclaredType.TypeInfo is UnknownType)
                node.DeclaredType.TypeInfo = node.Value.TypeInfo;
            else if (!Expect(node.Position, node.DeclaredType.TypeInfo, node.Value.TypeInfo))
                return false;

            node.TypeInfo = node.DeclaredType.TypeInfo;
            if (_scope.TryAddSymbol(node.Name, node.TypeInfo)) return true;

            Error(node.Position, "");
            return false;
        }

        protected override bool Visit(ASTFloatLiteral node)
        {
            node.TypeInfo = _cache.GetFloat(BaseType.UnknownSize);
            return true;
        }

        protected override bool Visit(ASTInfer node)
        {
            node.TypeInfo = _cache.GetUnknown();
            return true;
        }

        protected override bool Visit(ASTIntegerLiteral node)
        {
            node.TypeInfo = _cache.GetInt(BaseType.UnknownSize, node.Value < 0);
            return true;
        }

        protected override bool Visit(ASTProcedureType node)
        {
            var arguments = new List<ProcedureType.Argument>();

            foreach (var arg in node.Arguments)
            {
                if (!Visit(arg)) return false;
                if (!(arg.TypeInfo is TypeType c) || c.Type.Equals(_cache.GetUnknown()))
                {
                    Error(arg.Position, "Argument did not specify a valid type.");
                    return false;
                }

                arguments.Add(new ProcedureType.Argument(null, c.Type));
            }

            if (!Visit(node.Return)) return false;

            // TODO: Cache procedure types?
            node.TypeInfo = _cache.GetType(new ProcedureType(null, arguments, node.Return.TypeInfo));
            return true;
        }

        protected override bool Visit(ASTStructType node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTArrayType node)
        {
            if (!Visit(node.Child)) return false;

            var childType = node.Child.TypeInfo;
            if (!(childType is TypeType c) || c.Type.Equals(_cache.GetUnknown()))
            {
                Error(node.Position, "Can only construct and array type, for compile time known types.");
                return true;
            }

            node.TypeInfo = _cache.GetType(_cache.GetArray(c.Type));
            return true;
        }

        protected override bool Visit(ASTStringLiteral node)
        {
            node.TypeInfo = new StringType();
            return true;
        }

        protected override bool Visit(ASTLambda node)
        {
            _procedureStack.Push(node);
            _scope = new Scope { Parent = _scope };

            var arguments = new List<ProcedureType.Argument>(node.Arguments.Count());

            foreach (var arg in node.Arguments)
            {
                if (!Visit(arg))
                {
                    _procedureStack.Pop();
                    return false;
                }

                if (!(arg.TypeInfo is TypeType c) || c.Type.Equals(_cache.GetUnknown()))
                {
                    Error(arg.Position, "Argument did not specify a valid type.");
                    return false;
                }

                _scope.TryAddSymbol(arg.Name, c.Type);
                arguments.Add(new ProcedureType.Argument(arg.Name, c.Type));
            }

            var ret = node.Return;
            if (ret == null)
            {
                node.TypeInfo = new ProcedureType(null, arguments, _cache.GetVoid());
            }
            else if (Visit(ret))
            {
                if (!(ret.TypeInfo is TypeType c) || c.Type.Equals(_cache.GetUnknown()))
                {
                    Error(ret.Position, "Return was not specified as a valid type.");
                    return false;
                }

                node.TypeInfo = new ProcedureType(null, arguments, c.Type);
            }
            else
            {
                _procedureStack.Pop();
                return false;
            }

            if (!Visit(node.Block))
            {
                _procedureStack.Pop();
                return false;
            }

            _scope = _scope.Parent;
            _procedureStack.Pop();
            return true;
        }

        protected override bool Visit(ASTLambda.Argument node)
        {
            if (!Visit(node.Type)) return false;

            if (node.DefaultValue != null)
            {
                if (!Visit(node.DefaultValue)) return false;
                if (!Expect(node.Position, node.Type.TypeInfo, node.DefaultValue.TypeInfo)) return false;
            }

            node.TypeInfo = node.Type.TypeInfo;
            return true;
        }

        protected override bool Visit(ASTStructInitializer node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTEmptyInitializer node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTArrayInitializer node)
        {
            throw new NotImplementedException();
        }

        protected override bool Visit(ASTAdd node) => VisitArethetic(node);
        protected override bool Visit(ASTSub node) => VisitArethetic(node);
        protected override bool Visit(ASTDivide node) => VisitArethetic(node);
        protected override bool Visit(ASTTimes node) => VisitArethetic(node);

        // TODO: Is this really correct? Does modulo types function as other arethmetic operators
        protected override bool Visit(ASTModulo node) => VisitArethetic(node);

        protected override bool Visit(ASTAnd node) => VisitAndOr(node);
        protected override bool Visit(ASTOr node) => VisitAndOr(node);

        private bool VisitAndOr(ASTBinaryNode node)
        {
            if (!Visit(node.Left, node.Right)) return false;
            var left = node.Left.TypeInfo;
            var right = node.Right.TypeInfo;

            if (!(left is BooleanType && right is BooleanType))
            {
                Error(node.Position, $"Cannot add {left} and {right}");
                return false;
            }

            node.TypeInfo = left;
            return true;
        }
        
        private bool VisitArethetic(ASTBinaryNode node)
        {
            if (!Visit(node.Left, node.Right)) return false;
            var left = node.Left.TypeInfo;
            var right = node.Right.TypeInfo;

            if (CanImplecitCast(left, right))
            {
                node.TypeInfo = right;
            }
            else if (CanImplecitCast(right, left))
            {
                node.TypeInfo = left;
            }

            if (node.TypeInfo is IntegerType || node.TypeInfo is FloatType)
                return true;

            Error(node.Position, $"Cannot add {left} and {right}");
            return false;
        }

        protected override bool Visit(ASTGreaterThan node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = _cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTGreaterThanEqual node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = _cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTLessThan node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = _cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTLessThanEqual node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = _cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTNotEqual node)
        {
            if (!Visit(node.Left, node.Right)) return false;
            var left = node.Left.TypeInfo;
            var right = node.Right.TypeInfo;

            if (!left.Equals(right))
            {
                Error(node.Position, $"Cannot compare {left} and {right}");
                return false;
            }

            node.TypeInfo = _cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTEqual node)
        {
            if (!Visit(node.Left, node.Right)) return false;
            var left = node.Left.TypeInfo;
            var right = node.Right.TypeInfo;

            if (!left.Equals(right))
            {
                Error(node.Position, $"Cannot compare {left} and {right}");
                return false;
            }

            node.TypeInfo = _cache.GetBoolean();
            return true;
        }


        protected override bool Visit(ASTAs node)
        {
            if (!Visit(node.Left, node.Right)) return false;
            if (!(node.Right.TypeInfo is TypeType c) || c.Type.Equals(_cache.GetUnknown()))
            {
                Error(node.Position, "Can only cast to compile time types");
                return false;
            }

            node.TypeInfo = c.Type;
            return true;
        }

        protected override bool Visit(ASTCompilerCall node)
        {
            if (!Visit(node.Arguments)) return false;
            
            var pCount = p.Arguments.Count();
            var nCount = node.Arguments.Count();
            if (pCount != nCount)
            {
                Error(node.Position, $"Procedure requires {pCount} arguments, but was giving {nCount}");
                return false;
            }

            foreach (var (nArg, pArg) in node.Arguments.Zip(p.Arguments, (nArg, pArg) => (nArg, pArg)))
            {
                if (!CanImplecitCast(nArg.TypeInfo, pArg.Type))
                {
                    Error(node.Position, $"Passing wrong type to procedure, expected {pArg.Type}, but got {nArg.TypeInfo}.");
                    return false;
                }
            }

            node.TypeInfo = p.Return;
            return true;
        }

        protected override bool Visit(ASTDereference node)
        {
            if (!Visit(node.Child)) return false;

            var childType = node.Child.TypeInfo;
            if (childType is PointerType p)
            {
                node.TypeInfo = p.RefersTo;
                return true;
            }

            Error(node.Position, $"Can't dereference none pointer {childType}");
            return false;
        }

        protected override bool Visit(ASTReference node)
        {
            if (!Visit(node.Child)) return false;
            node.TypeInfo = _cache.GetPointer(node.TypeInfo);
            return true;
        }

        protected override bool Visit(ASTIndexing node)
        {
            if (!Visit(node.Child))     return false;
            if (!Visit(node.Arguments)) return false;

            var childType = node.Child.TypeInfo;

            if (!(childType is ArrayType a))
            {
                Error(node.Position, $"Can't index none array {childType}, yet");
                return false;
            }

            var count = node.Arguments.Count();
            if (count != 1)
            {
                Error(node.Position, $"Can't index none array {childType}, yet");
                return false;
            }

            var first = node.Arguments.First();
            if (!(first.TypeInfo is IntegerType))
            {
                Error(node.Position, "Can't index array with a none Interger value");
                return false;
            }

            node.TypeInfo = a.ItemType;
            return true;
        }

        protected override bool Visit(ASTCall node)
        {
            if (!Visit(node.Child)) return false;
            if (!Visit(node.Arguments)) return false;

            var childType = node.Child.TypeInfo;

            if (!(childType is ProcedureType p))
            {
                Error(node.Position, $"Can't call none procedure {childType}");
                return false;
            }

            var pCount = p.Arguments.Count();
            var nCount = node.Arguments.Count();
            if (pCount != nCount)
            {
                Error(node.Position, $"Procedure requires {pCount} arguments, but was giving {nCount}");
                return false;
            }

            foreach (var (nArg, pArg) in node.Arguments.Zip(p.Arguments, (nArg, pArg) => (nArg, pArg)))
            {
                if (!CanImplecitCast(nArg.TypeInfo, pArg.Type))
                {
                    Error(node.Position, $"Passing wrong type to procedure, expected {pArg.Type}, but got {nArg.TypeInfo}.");
                    return false;
                }
            }
            
            node.TypeInfo = p.Return;
            return true;
        }

        protected override bool Visit(ASTNegative node)
        {
            if (!Visit(node.Child)) return false;

            var childType = node.Child.TypeInfo;
            if (childType is FloatType f)
            {
                node.TypeInfo = f;
                return true;
            }

            if (childType is IntegerType i)
            {
                if (!i.Signed)
                {
                    if (i.Size != BaseType.UnknownSize)
                    {
                        Error(node.Position, $"Cannot negate {childType}");
                        return false;
                    }

                    // If the size of the unsigned interger is unknown, then we are free to convert between signed and unsigned
                    node.TypeInfo = _cache.GetInt(BaseType.UnknownSize, true);
                    return true;
                }

                node.TypeInfo = i;
                return true;
            }

            Error(node.Position, $"Cannot negate {childType}");
            return false;
        }

        protected override bool Visit(ASTNot node)
        {
            if (!Visit(node.Child)) return false;

            var childType = node.Child.TypeInfo;
            if (!(childType is BooleanType))
            {
                Error(node.Position, $"Cannot not none Boolean type {childType}");
                return false;
            }

            node.TypeInfo = childType;
            return true;
        }

        protected override bool Visit(ASTPositive node)
        {
            if (!Visit(node.Child)) return false;

            var childType = node.Child.TypeInfo;
            if (childType is FloatType f)
            {
                node.TypeInfo = f;
                return true;
            }

            if (childType is IntegerType i)
            {
                node.TypeInfo = i;
                return true;
            }

            Error(node.Position, $"Cannot do unary positive on {childType}");
            return false;
        }

        protected override bool Visit(ASTSymbol node)
        {
            if (_scope.TryGetTypeOf(node.Name, out var result))
            {
                node.TypeInfo = result;
                return true;
            }

            Error(node.Position, $"Nothing defined called {node.Name}");
            return false;
        }


        protected override bool Visit(ASTDot node)
        {
            throw new NotImplementedException();
        }

        private bool CanImplecitCast(BaseType type, BaseType cast)
        {
            if (type is IntegerType iType && cast is IntegerType iCast)
            {
                if (iType.Signed == iCast.Signed && iType.Size <= iCast.Size) return true;
                return iType.Size == BaseType.UnknownSize && iCast.Signed;
            }

            if (type is FloatType fType && cast is FloatType fCast)
            {
                return fType.Size <= fCast.Size;
            }

            return type.Equals(cast);
        }

        private bool Expect(Position position, BaseType expected, BaseType actual)
        {
            if (expected.Equals(actual)) return true;

            Error(position, $"Expected {expected}, but got {actual}.");
            return false;
        }

        private void Error(Position position, string message) => _compiler.ReportError(position, nameof(TypeChecker), message);
    }
}
