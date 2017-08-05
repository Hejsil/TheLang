namespace TheLang.Semantics.TypeChecking.Types
{
    public class StringType : BaseType
    {
        protected bool Equals(StringType other) => true;
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((StringType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
            }
        }

        public override string ToString() => "String";
        
        public StringType()
            : base(64 * 2)
        { }
    }
}
