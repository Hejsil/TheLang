using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTModulo : ASTBinaryNode
    {
        public ASTModulo(Position position)
            : base(position)
        { }
    }
}