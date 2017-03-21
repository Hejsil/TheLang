using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class BinaryOperator : Expression
    {
        public BinaryOperator(Position position) 
            : base(position)
        { }

        public Expression Left { get; set; }
        public Expression Right { get; set; }
        public BinaryOperatorKind Kind { get; set; }
        public int Priority => ((int)Kind) >> 4;

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
