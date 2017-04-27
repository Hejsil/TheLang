using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Return : UnaryNode
    {
        public Return(Position position)
            : base(position)
        { }
    }
}