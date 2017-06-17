using TheLang.Syntax;

namespace TheLang.AST.Bases
{
    public abstract class ASTBinaryNode : ASTNode
    {
        protected ASTBinaryNode(Position position) 
            : base(position)
        { }

        public ASTNode Left { get; set; }
        public ASTNode Right { get; set; }
    }
}
