using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTPositive : ASTUnaryNode
    {
        public ASTPositive(Position position)
            : base(position)
        { }
    }
}