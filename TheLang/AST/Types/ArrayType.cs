using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public class ArrayType : TypeNode
    {
        public ArrayType(Position position, int dimensions, TypeNode elementType) 
            : base(position)
        {
            Dimensions = dimensions;
            ElementType = elementType;
        }

        public int Dimensions { get; }
        public TypeNode ElementType { get; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
