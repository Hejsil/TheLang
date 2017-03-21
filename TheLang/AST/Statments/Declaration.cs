using TheLang.AST.Types;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Declaration : Node
    {
        public Declaration(Position position, string name, TypeNode declaredType) 
            : base(position)
        {
            Name = name;
            DeclaredType = declaredType;
        }

        public string Name { get; set; }
        public TypeNode DeclaredType { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
