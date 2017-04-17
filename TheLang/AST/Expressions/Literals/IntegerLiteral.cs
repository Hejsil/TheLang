using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class IntegerLiteral : Node
    {
        public IntegerLiteral(Position position, long value) 
            : base(position)
        {
            Value = value;
        }

        public long Value { get; set; }
    }
}
