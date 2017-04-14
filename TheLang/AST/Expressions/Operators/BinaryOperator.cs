using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class BinaryOperator : Expression
    {
        public BinaryOperator(Position position, BinaryOperatorKind kind) 
            : base(position)
        {
            Kind = kind;
        }

        public Expression Left { get; set; }
        public Expression Right { get; set; }
        public BinaryOperatorKind Kind { get; }
        public int Priority => ((int)Kind) >> 4;

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
