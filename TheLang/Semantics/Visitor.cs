using System;
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
                case BlockBodyProcedure n:
                    return Visit(n);
                case CompositTypeLiteral n:
                    return Visit(n);
                case ExpressionBodyProcedure n:
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
                case Declaration n:
                    return Visit(n);
                case Variable n:
                    return Visit(n);
                case ArrayPostfix n:
                    return Visit(n);
                case NeedsToBeInfered n:
                    return Visit(n);
                case ProcedureLiteral n:
                    return Visit(n);
                case ProgramNode n:
                    return Visit(n);
                case FileNode n:
                    return Visit(n);
            }

            throw new ArgumentException("The argument was of a type not supported by this visitor", nameof(node));
        }

        public abstract bool Visit(StringLiteral node);
        public abstract bool Visit(Variable node);
        public abstract bool Visit(NeedsToBeInfered node);
        public abstract bool Visit(IntegerLiteral node);
        public abstract bool Visit(FloatLiteral node);
        public abstract bool Visit(ArrayPostfix node);
        public abstract bool Visit(TupleLiteral node);
        public abstract bool Visit(Declaration node);
        public abstract bool Visit(BinaryOperator node);
        public abstract bool Visit(UnaryOperator node);
        public abstract bool Visit(Symbol node);
        public abstract bool Visit(CompositTypeLiteral node);
        public abstract bool Visit(Call node);
        public abstract bool Visit(ProcedureLiteral node);

        public virtual bool Visit(ProgramNode node) => node.Files.All(Visit);
        public virtual bool Visit(FileNode node) => node.Declarations.All(Visit);
        public virtual bool Visit(CodeBlock node) => node.Statements.All(Visit);
    }
}
