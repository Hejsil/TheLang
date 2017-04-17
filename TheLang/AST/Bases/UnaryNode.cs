using TheLang.Syntax;

namespace TheLang.AST.Bases
{
    public abstract class UnaryNode : Node
    {
        protected UnaryNode(Position position) 
            : base(position)
        { }

        public Node Child { get; set; }
    }
}
