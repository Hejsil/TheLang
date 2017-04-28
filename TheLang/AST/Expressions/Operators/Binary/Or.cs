using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class Or : BinaryNode
    {
        public Or(Position position)
            : base(position)
        { }
    }
}