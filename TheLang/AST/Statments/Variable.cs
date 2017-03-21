using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Variable : Node
    {
        public Variable(Position position, Declaration declaration, Expression expression, bool isConstant)
            : base(position)
        {
            Declaration = declaration;
            Expression = expression;
            IsConstant = isConstant;
        }

        public Declaration Declaration { get; set; }
        public Expression Expression { get; set; }
        public bool IsConstant { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
