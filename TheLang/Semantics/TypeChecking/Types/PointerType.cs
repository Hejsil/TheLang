using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class PointerType : BaseType
    {
        protected bool Equals(PointerType other) => base.Equals(other) && Equals(RefersTo, other.RefersTo);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PointerType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (RefersTo != null ? RefersTo.GetHashCode() : 0);
            }
        }

        public override string ToString() => $"@{RefersTo}";

        public PointerType(BaseType refersTo)
            : base(64)
        {
            RefersTo = refersTo;
        }

        public BaseType RefersTo { get; }
    }
}
