using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Negative : UnaryNode
    {
        public Negative(Position position)
            : base(position)
        { }
    }
}