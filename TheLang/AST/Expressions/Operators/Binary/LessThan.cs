using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class LessThan : BinaryNode
    {
        public LessThan(Position position)
            : base(position)
        { }
    }
}