using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class IntegerType : Type
    {
        public IntegerType(int size, bool isSigned) 
            : base(size)
        {
            IsSigned = isSigned;
        }

        public bool IsSigned { get; }

        public override bool Equals(object obj) => 
            obj is IntegerType i && 
            IsSigned == i.IsSigned && 
            Size == i.Size;

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => IsSigned ? $"Int{Size}" : $"UInt{Size}";
    }
}
