using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public abstract class TypeInfo
    {
        protected TypeInfo(int size)
        {
            Size = size;
        }

        public int Size { get; }

        public abstract override bool Equals(object obj);
        public abstract override string ToString();
        public override int GetHashCode() => ToString().GetHashCode();
    }
}
