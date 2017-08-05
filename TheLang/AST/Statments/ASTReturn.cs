using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class ASTReturn : ASTUnaryNode
    {
        public ASTReturn(Position position)
            : base(position)
        { }
    }
}