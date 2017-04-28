using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class LessThanEqual : BinaryNode
    {
        public LessThanEqual(Position position)
            : base(position)
        { }
    }
}