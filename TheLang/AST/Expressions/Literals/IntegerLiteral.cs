using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class IntegerLiteral : Expression
    {
        public IntegerLiteral(Position position, long value) 
            : base(position)
        {
            Value = value;
        }

        public long Value { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
