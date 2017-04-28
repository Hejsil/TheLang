using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Positive : UnaryNode
    {
        public Positive(Position position)
            : base(position)
        { }
    }
}