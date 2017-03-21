using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public abstract class TypeNode : Node
    {
        protected TypeNode(Position position) 
            : base(position)
        { }
    }
}
