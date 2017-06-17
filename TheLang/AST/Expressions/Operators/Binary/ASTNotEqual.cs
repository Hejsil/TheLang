using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTNotEqual : ASTBinaryNode
    {
        public ASTNotEqual(Position position)
            : base(position)
        { }
    }
}