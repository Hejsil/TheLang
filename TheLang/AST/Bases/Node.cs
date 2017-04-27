using TheLang.Semantics.TypeChecking;
using TheLang.Syntax;

namespace TheLang.AST.Bases
{
    public abstract class Node
    {
        protected Node(Position position)
        {
            Position = position;
        }

        public Position Position { get; }
        public TypeInfo Type { get; set; }
    }
}
