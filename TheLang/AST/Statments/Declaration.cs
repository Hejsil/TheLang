using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Declaration : Node
    {
        public Declaration(Position position, string name) 
            : base(position)
        {
            Name = name;
        }

        public string Name { get; set; }
        public Expression DeclaredType { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
