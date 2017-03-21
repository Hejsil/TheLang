using TheLang.Syntax;

namespace TheLang.AST
{
    public abstract class Node
    {
        protected Node(Position position)
        {
            Position = position;
        }

        public Position Position { get; }

        public abstract bool Accept(IVisitor visitor);
    }
}
