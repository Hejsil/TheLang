using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Binary
{
    public class Equal : BinaryNode
    {
        public Equal(Position position)
            : base(position)
        { }
    }
}