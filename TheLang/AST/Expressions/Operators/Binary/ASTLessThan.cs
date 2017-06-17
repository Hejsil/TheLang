using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTLessThan : ASTBinaryNode
    {
        public ASTLessThan(Position position)
            : base(position)
        { }
    }
}