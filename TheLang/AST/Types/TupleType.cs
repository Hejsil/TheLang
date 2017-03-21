using System.Collections.Generic;
using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public class TupleType : TypeNode
    {
        public TupleType(Position position, IEnumerable<TypeNode> itemTypes) 
            : base(position)
        {
            ItemTypes = itemTypes;
        }

        public IEnumerable<TypeNode> ItemTypes { get; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
