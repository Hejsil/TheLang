using TheLang.Semantics.TypeChecking;
using TheLang.Syntax;

namespace TheLang.AST.Bases
{
    public abstract class ASTNode
    {
        protected ASTNode(Position position)
        {
            Position = position;
        }

        public Position Position { get; set; }
        public TypeInfo Type { get; set; }
    }
}
