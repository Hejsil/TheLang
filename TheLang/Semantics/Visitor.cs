using System;
using System.Collections.Generic;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;

namespace TheLang.Semantics
{
    public abstract class Visitor
    {
        public bool Visit(dynamic node) => Visit(node);

        protected abstract bool Visit(Indexing node);
        protected abstract bool Visit(StringLiteral node);
        protected abstract bool Visit(NeedsToBeInfered node);
        protected abstract bool Visit(IntegerLiteral node);
        protected abstract bool Visit(FloatLiteral node);
        protected abstract bool Visit(ArrayPostfix node);
        protected abstract bool Visit(TupleLiteral node);
        protected abstract bool Visit(Variable node);
        protected abstract bool Visit(Infer node);
        protected abstract bool Visit(Declaration node);
        protected abstract bool Visit(BinaryOperator node);
        protected abstract bool Visit(UnaryOperator node);
        protected abstract bool Visit(Symbol node);
        protected abstract bool Visit(TypeLiteral node);
        protected abstract bool Visit(StructType node);
        protected abstract bool Visit(Call node);
        protected abstract bool Visit(ProcedureTypeNode node);
        protected abstract bool Visit(Return node);
        protected abstract bool Visit(TypedProcedureLiteral node);

        protected virtual bool Visit(ProgramNode node) => VisitCollection(node.Files);
        protected virtual bool Visit(FileNode node) => VisitCollection(node.Declarations);
        protected virtual bool Visit(CodeBlock node) => VisitCollection(node.Statements);

        protected bool VisitCollection<T>(IEnumerable<T> nodes) where T : Node
        {
            var hasFailed = false;

            foreach (var node in nodes)
            {
                if (!Visit(node))
                    hasFailed = true;
            }

            return hasFailed;
        }
    }
}
