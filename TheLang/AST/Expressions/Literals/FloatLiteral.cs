using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class FloatLiteral : Expression
    {
        public FloatLiteral(Position position, double value) 
            : base(position)
        {
            Value = value;
        }

        public double Value { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
