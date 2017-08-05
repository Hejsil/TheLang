using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ArrayType : BaseType
    {
        protected bool Equals(ArrayType other) => base.Equals(other) && Equals(ItemType, other.ItemType);
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ArrayType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ItemType != null ? ItemType.GetHashCode() : 0);
            }
        }

        public override string ToString() => $"[]{ItemType}";

        public ArrayType(BaseType itemType)
            : base(64 * 2)
        {
            ItemType = itemType;
        }

        public BaseType ItemType { get; }
    }
}
