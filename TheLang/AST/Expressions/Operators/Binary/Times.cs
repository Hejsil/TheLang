using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class Times : BinaryNode
    {
        public Times(Position position)
            : base(position)
        { }
    }
}