using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Not : UnaryNode
    {
        public Not(Position position)
            : base(position)
        { }
    }
}