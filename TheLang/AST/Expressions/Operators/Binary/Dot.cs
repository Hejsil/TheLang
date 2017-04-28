using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class Dot : BinaryNode
    {
        public Dot(Position position)
            : base(position)
        { }

        public new Symbol Right => (Symbol)base.Right;
    }
}