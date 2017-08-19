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
        private TypeCache Cache => _compiler.TypeCache;

        private readonly Compiler _compiler;
        private readonly Stack<ASTLambda> _procedureStack = new Stack<ASTLambda>();
        private Scope _scope = new Scope();

        public TypeChecker(Compiler compiler)
        {
            _compiler = compiler;

            // Predefined types
            _scope.TryAddSymbol("I64", Cache.GetType(Cache.GetInt(64, true)));
            _scope.TryAddSymbol("F64", Cache.GetType(Cache.GetFloat(64)));
            _scope.TryAddSymbol("String", Cache.GetType(Cache.GetString()));
            _scope.TryAddSymbol("Type", Cache.GetType(Cache.GetType(Cache.GetUnknown())));
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
            node.TypeInfo = Cache.GetFloat(BaseType.UnknownSize);
            return true;
        }

        protected override bool Visit(ASTInfer node)
        {
            node.TypeInfo = Cache.GetUnknown();
            return true;
        }

        protected override bool Visit(ASTIntegerLiteral node)
        {
            node.TypeInfo = Cache.GetInt(BaseType.UnknownSize, node.Value < 0);
            return true;
        }

        protected override bool Visit(ASTProcedureType node)
        {
            var arguments = new List<ProcedureType.Argument>();

            foreach (var arg in node.Arguments)
            {
                if (!Visit(arg)) return false;
                if (!(arg.TypeInfo is TypeType c) || c.Type.Equals(Cache.GetUnknown()))
                {
                    Error(arg.Position, "Argument did not specify a valid type.");
                    return false;
                }

                arguments.Add(new ProcedureType.Argument(null, c.Type));
            }

            if (!Visit(node.Return)) return false;

            // TODO: Cache procedure types?
            node.TypeInfo = Cache.GetType(new ProcedureType(arguments, node.Return.TypeInfo));
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
            if (!(childType is TypeType c) || c.Type.Equals(Cache.GetUnknown()))
            {
                Error(node.Position, "Can only construct and array type, for compile time known types.");
                return true;
            }

            node.TypeInfo = Cache.GetType(Cache.GetArray(c.Type));
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

                if (!(arg.TypeInfo is TypeType c) || c.Type.Equals(Cache.GetUnknown()))
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
                node.TypeInfo = new ProcedureType(arguments, Cache.GetVoid());
            }
            else if (Visit(ret))
            {
                if (!(ret.TypeInfo is TypeType c) || c.Type.Equals(Cache.GetUnknown()))
                {
                    Error(ret.Position, "Return was not specified as a valid type.");
                    return false;
                }

                node.TypeInfo = new ProcedureType(arguments, c.Type);
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

            node.TypeInfo = Cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTGreaterThanEqual node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = Cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTLessThan node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = Cache.GetBoolean();
            return true;
        }

        protected override bool Visit(ASTLessThanEqual node)
        {
            if (!VisitArethetic(node)) return false;

            node.TypeInfo = Cache.GetBoolean();
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

            node.TypeInfo = Cache.GetBoolean();
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

            node.TypeInfo = Cache.GetBoolean();
            return true;
        }


        protected override bool Visit(ASTAs node)
        {
            if (!Visit(node.Left, node.Right)) return false;
            if (!(node.Right.TypeInfo is TypeType c) || c.Type.Equals(Cache.GetUnknown()))
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
            
            var success = CheckMany(
                node.Position,
                node.Arguments,
                node.Procedure.Type.Arguments,
                (actual, expected) => CanImplecitCast(actual.TypeInfo, expected.Type),
                (actual, expected) => $"Procedure requires {expected} arguments, but was giving {actual}",
                (actual, expected) => $"Passing wrong type to procedure, expected {expected.Type}, but got {actual.TypeInfo}.");

            if (!success)
                return false;

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
            node.TypeInfo = Cache.GetPointer(node.TypeInfo);
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


            var success = CheckMany(
                node.Position,
                node.Arguments,
                p.Arguments,
                (actual, expected) => CanImplecitCast(actual.TypeInfo, expected.Type),
                (actual, expected) => $"Procedure requires {expected} arguments, but was giving {actual}",
                (actual, expected) => $"Passing wrong type to procedure, expected {expected.Type}, but got {actual.TypeInfo}.");

            if (!success)
                return false;
            
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
                    node.TypeInfo = Cache.GetInt(BaseType.UnknownSize, true);
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

        private bool CheckMany<T1, T2>(
            Position position,
            IEnumerable<T1> enu1, 
            IEnumerable<T2> enu2,
            Func<T1, T2, bool> check, 
            Func<int, int, string> countErrorBuilder,
            Func<T1, T2, string> checkErrorBuilder)
        {
            var c1 = enu1.Count();
            var c2 = enu2.Count();
            if (c1 != c2)
            {
                Error(position, countErrorBuilder(c1, c2));
                return false;
            }

            foreach (var (e1, e2) in enu1.Zip(enu2, (e1, e2) => (e1, e2)))
            {
                if (!check(e1, e2))
                {
                    Error(position, checkErrorBuilder(e1, e2));
                    return false;
                }
            }

            return true;
        }

        private static bool CanImplecitCast(BaseType type, BaseType cast)
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
