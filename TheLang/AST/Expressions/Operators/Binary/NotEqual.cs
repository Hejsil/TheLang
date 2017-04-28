using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class NotEqual : BinaryNode
    {
        public NotEqual(Position position)
            : base(position)
        { }
    }
}