using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTNegative : ASTUnaryNode
    {
        public ASTNegative(Position position)
            : base(position)
        { }
    }
}