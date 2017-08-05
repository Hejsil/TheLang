using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class BooleanType : BaseType
    {
        protected bool Equals(BooleanType other) => true;
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BooleanType) obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ GetType().GetHashCode();
            }
        }

        public override string ToString() => $"Bool";

        public BooleanType() 
            : base(8)
        { }
    }
}
