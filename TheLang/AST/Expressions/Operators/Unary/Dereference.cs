using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Dereference : UnaryNode
    {
        public Dereference(Position position)
            : base(position)
        { }
    }
}