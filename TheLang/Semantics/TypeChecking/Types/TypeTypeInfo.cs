using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class TypeTypeInfo : TypeInfo
    {
        public TypeInfo Type { get; }

        public TypeTypeInfo(TypeInfo type) 
            : base(PointerSize)
        {
            Type = type;
        }

        protected bool Equals(TypeTypeInfo other) => Equals(Type, other.Type);

        public override bool Equals(object obj) =>
            base.Equals(obj) && Equals((TypeTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .Hash(Type)
                .Value;

        public override string ToString() => $"Type({Type})";
    }
}
