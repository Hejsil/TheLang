using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class UniqueReference : UnaryNode
    {
        public UniqueReference(Position position)
            : base(position)
        { }
    }
}