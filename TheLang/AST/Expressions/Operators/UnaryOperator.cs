using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class UnaryOperator : Expression
    {
        public UnaryOperator(Position position) 
            : base(position)
        { }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
