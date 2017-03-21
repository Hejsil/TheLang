using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public class PointerType : TypeNode
    {
        public PointerType(Position position, TypeNode poitingTo, Kind type)
            : base(position)
        {
            PointingTo = poitingTo;
            Type = type;
        }

        public Kind Type { get; }
        public TypeNode PointingTo { get; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);

        public enum Kind
        {
            Normal,
            Shared,
            Unique
        }
    }
}
