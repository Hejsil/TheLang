using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Return : ASTUnaryNode
    {
        public Return(Position position)
            : base(position)
        { }
    }
}