using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTDot : ASTBinaryNode
    {
        public ASTDot(Position position)
            : base(position)
        { }

        public new ASTSymbol Right => (ASTSymbol)base.Right;
    }
}