using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public abstract class NameableType : BaseType
    {
        protected bool Equals(NameableType other) => base.Equals(other) && string.Equals(Name, other.Name);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NameableType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public string Name { get; }

        protected NameableType(int size, string name)
            : base(size)
        {
            Name = name;
        }
    }
}
