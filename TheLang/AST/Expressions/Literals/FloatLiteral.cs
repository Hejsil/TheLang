using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class FloatLiteral : Node
    {
        public FloatLiteral(Position position, double value) 
            : base(position)
        {
            Value = value;
        }

        public double Value { get; set; }
    }
}
