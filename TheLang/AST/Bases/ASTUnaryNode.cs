using TheLang.Syntax;

namespace TheLang.AST.Bases
{
    public abstract class ASTUnaryNode : ASTNode
    {
        protected ASTUnaryNode(Position position) 
            : base(position)
        { }

        public ASTNode Child { get; set; }
    }
}
