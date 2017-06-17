using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTAnd : ASTBinaryNode
    {
        public ASTAnd(Position position)
            : base(position)
        { }
    }
}