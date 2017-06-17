using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTGreaterThanEqual : ASTBinaryNode
    {
        public ASTGreaterThanEqual(Position position)
            : base(position)
        { }
    }
}