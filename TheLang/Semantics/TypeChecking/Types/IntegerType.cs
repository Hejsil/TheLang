using System;
using System.Numerics;
using PeterO.Numbers;
using TheLang.Util;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class IntegerType : BaseType
    {
        protected bool Equals(IntegerType other) => base.Equals(other) && Signed == other.Signed;
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((IntegerType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Signed.GetHashCode();
            }
        }

        public override string ToString() => Signed ? $"I{Size}" : $"U{Size}";

        public bool Signed { get; }
        public EInteger Max => ENumbers.ITwo.Pow(Size - Convert.ToInt32(Signed)) - 1;
        public EInteger Min => -ENumbers.ITwo.Pow(Size - 1) * Convert.ToInt32(Signed);

        public IntegerType(int size, bool signed)
            : base(size)
        {
            Signed = signed;
        }
    }
}
