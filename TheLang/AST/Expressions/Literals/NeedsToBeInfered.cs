using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class NeedsToBeInfered : Node
    {
        public NeedsToBeInfered(Position position) 
            : base(position)
        { }
    }
}
