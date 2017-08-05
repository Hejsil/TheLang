using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class UnknownType : BaseType
    {
        protected bool Equals(UnknownType other) => true;
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((UnknownType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
            }
        }

        public override string ToString() => $"???";

        public UnknownType() 
            : base(UnknownSize)
        { }
    }
}
