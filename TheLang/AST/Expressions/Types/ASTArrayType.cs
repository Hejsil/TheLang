using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Types
{
    public class ASTArrayType : ASTUnaryNode
    {
        public ASTArrayType(Position position)
            : base(position)
        { }

        public ASTNode Size { get; set; }
    }
}
