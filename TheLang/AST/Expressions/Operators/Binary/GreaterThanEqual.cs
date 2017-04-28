using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class GreaterThanEqual : BinaryNode
    {
        public GreaterThanEqual(Position position)
            : base(position)
        { }
    }
}