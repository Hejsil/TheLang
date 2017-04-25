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
        public bool Visit(Node node)
        {
            switch (node)
            {
                case TypedProcedureLiteral n:
                    return Visit(n);
                case CompositTypeLiteral n:
                    return Visit(n);
                case FloatLiteral n:
                    return Visit(n);
                case IntegerLiteral n:
                    return Visit(n);
                case StringLiteral n:
                    return Visit(n);
                case TupleLiteral n:
                    return Visit(n);
                case BinaryOperator n:
                    return Visit(n);
                case UnaryOperator n:
                    return Visit(n);
                case Call n:
                    return Visit(n);
                case Symbol n:
                    return Visit(n);
                case CodeBlock n:
                    return Visit(n);
                case Variable n:
                    return Visit(n);
                case Declaration n:
                    return Visit(n);
                case ArrayPostfix n:
                    return Visit(n);
                case NeedsToBeInfered n:
                    return Visit(n);
                case ProcedureTypeNode n:
                    return Visit(n);
                case ProgramNode n:
                    return Visit(n);
                case FileNode n:
                    return Visit(n);
            }

            throw new ArgumentException("The argument was of a type not supported by this visitor", nameof(node));
        }

        protected abstract bool Visit(StringLiteral node);
        protected abstract bool Visit(Variable node);
        protected abstract bool Visit(NeedsToBeInfered node);
        protected abstract bool Visit(IntegerLiteral node);
        protected abstract bool Visit(FloatLiteral node);
        protected abstract bool Visit(ArrayPostfix node);
        protected abstract bool Visit(TupleLiteral node);
        protected abstract bool Visit(Declaration node);
        protected abstract bool Visit(BinaryOperator node);
        protected abstract bool Visit(UnaryOperator node);
        protected abstract bool Visit(Symbol node);
        protected abstract bool Visit(CompositTypeLiteral node);
        protected abstract bool Visit(Call node);
        protected abstract bool Visit(ProcedureTypeNode node);

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
