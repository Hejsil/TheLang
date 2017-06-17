using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTParentheses : ASTUnaryNode
    {
        public ASTParentheses(Position position)
            : base(position)
        { }
    }
}