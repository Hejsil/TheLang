using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class FloatType : Type
    {
        public FloatType(int size) 
            : base(size)
        {
        }

        public override bool Equals(object obj) => 
            obj is FloatType f && 
            Size == f.Size;

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => $"Float{Size}";
    }
}
