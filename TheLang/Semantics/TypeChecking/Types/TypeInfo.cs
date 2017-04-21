using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public abstract class TypeInfo
    {
        public const int Bit64 = 64;
        public const int NeedToBeInferedSize = -1;
        public const int PointerSize = Bit64;
        public const int Int64Size = Bit64;
        public const int Float64Size = Bit64;
        public const int ArraySize = PointerSize + Int64Size;

        protected TypeInfo(int size)
        {
            Size = size;
        }

        public int Size { get; }

        protected bool Equals(TypeInfo other) =>
            Size == other.Size;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TypeInfo)obj);
        }

        public abstract override string ToString();

        public override int GetHashCode() => 
            HashCodeHelper.Initialize()
                .Hash(Size)
                .Value;
    }
}
