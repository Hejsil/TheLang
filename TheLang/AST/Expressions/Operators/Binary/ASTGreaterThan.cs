using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTGreaterThan : ASTBinaryNode
    {
        public ASTGreaterThan(Position position)
            : base(position)
        { }
    }
}