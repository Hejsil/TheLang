using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTLessThanEqual : ASTBinaryNode
    {
        public ASTLessThanEqual(Position position)
            : base(position)
        { }
    }
}