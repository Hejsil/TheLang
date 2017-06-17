using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTReference : ASTUnaryNode
    {
        public ASTReference(Position position)
            : base(position)
        { }
    }
}