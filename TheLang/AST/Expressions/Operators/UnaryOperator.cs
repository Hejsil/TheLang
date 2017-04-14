using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class UnaryOperator : Expression
    {
        public UnaryOperator(Position position, UnaryOperatorKind kind) 
            : base(position)
        {
            Kind = kind;
        }


        public Expression Child { get; set; }
        public UnaryOperatorKind Kind { get; set; }
        
        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
