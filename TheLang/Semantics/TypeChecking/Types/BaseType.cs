namespace TheLang.Semantics.TypeChecking.Types
{
    public abstract class BaseType
    {
        public const int UnknownSize = -1;

        public int Size { get; }

        protected BaseType(int size) => Size = size;

        protected bool Equals(BaseType other) => Size == other.Size;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BaseType)obj);
        }

        public override int GetHashCode() => Size;
    }
}
