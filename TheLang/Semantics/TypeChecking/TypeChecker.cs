using System;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeChecker : Visitor
    {
        private readonly Compiler _compiler;

        public TypeChecker(Compiler compiler)
        {
            _compiler = compiler;
        }

        public override bool Visit(StringLiteral node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(Variable node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(NeedsToBeInfered node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(IntegerLiteral node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(FloatLiteral node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(ArrayPostfix node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(TupleLiteral node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(Declaration node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(BinaryOperator node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(UnaryOperator node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(Symbol node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(CompositTypeLiteral node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(Call node)
        {
            throw new NotImplementedException();
        }

        public override bool Visit(ProcedureLiteral node)
        {
            throw new NotImplementedException();
        }
    }
}
