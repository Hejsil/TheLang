using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Parentheses : UnaryNode
    {
        public Parentheses(Position position)
            : base(position)
        { }
    }
}