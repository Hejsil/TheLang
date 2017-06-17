using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTNot : ASTUnaryNode
    {
        public ASTNot(Position position)
            : base(position)
        { }
    }
}