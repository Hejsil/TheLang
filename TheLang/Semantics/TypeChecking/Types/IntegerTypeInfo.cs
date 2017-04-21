using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class IntegerTypeInfo : TypeInfo
    {
        public IntegerTypeInfo(int size, bool isSigned) 
            : base(size)
        {
            IsSigned = isSigned;
        }

        public bool IsSigned { get; }

        protected bool Equals(IntegerTypeInfo other) => IsSigned == other.IsSigned;

        public override bool Equals(object obj) => 
            base.Equals(obj) && 
            Equals((IntegerTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .Hash(IsSigned)
                .Value;

        public override string ToString() => IsSigned ? $"Int{Size}" : $"UInt{Size}";
    }
}
