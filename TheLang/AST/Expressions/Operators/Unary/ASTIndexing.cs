using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTIndexing : ASTUnaryNode
    {
        public ASTIndexing(Position position)
            : base(position)
        { }

        public ASTNode Argument { get; set; }
    }
}