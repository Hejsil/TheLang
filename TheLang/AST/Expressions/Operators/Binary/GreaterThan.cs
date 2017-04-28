using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class GreaterThan : BinaryNode
    {
        public GreaterThan(Position position)
            : base(position)
        { }
    }
}