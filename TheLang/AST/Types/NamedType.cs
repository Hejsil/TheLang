using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public class NamedType : TypeNode
    {
        public NamedType(Position position, string name) 
            : base(position)
        {
            Name = name;
        }
        
        public string Name { get; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
