using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class ASTEqual : ASTBinaryNode
    {
        public ASTEqual(Position position)
            : base(position)
        { }
    }
}