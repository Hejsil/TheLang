using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Reference : UnaryNode
    {
        public Reference(Position position)
            : base(position)
        { }
    }
}