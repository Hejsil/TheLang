using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTOr : ASTBinaryNode
    {
        public ASTOr(Position position)
            : base(position)
        { }
    }
}