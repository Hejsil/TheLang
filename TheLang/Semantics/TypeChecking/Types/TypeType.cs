using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class TypeType : BaseType
    {
        protected bool Equals(TypeType other) => base.Equals(other) && Equals(Type, other.Type);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TypeType)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }

        public override string ToString() => $"Type";


        public TypeType(BaseType type)
            : base(64)
        {
            Type = type;
        }

        public BaseType Type { get; }
    }
}
