using TheLang.Syntax;

namespace TheLang.AST.Bases
{
    public abstract class BinaryNode : Node
    {
        protected BinaryNode(Position position) 
            : base(position)
        { }

        public Node Left { get; set; }
        public Node Right { get; set; }
    }
}
