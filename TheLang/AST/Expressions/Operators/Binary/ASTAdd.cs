using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTAdd : ASTBinaryNode
    {
        public ASTAdd(Position position)
            : base(position)
        { }
    }
}