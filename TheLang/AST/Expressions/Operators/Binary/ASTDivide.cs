using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTDivide : ASTBinaryNode
    {
        public ASTDivide(Position position)
            : base(position)
        { }
    }
}