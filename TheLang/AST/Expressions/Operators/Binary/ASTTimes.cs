using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTTimes : ASTBinaryNode
    {
        public ASTTimes(Position position)
            : base(position)
        { }
    }
}