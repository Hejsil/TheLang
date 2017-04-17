using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ArrayType : Type
    {
        public ArrayType(int size, Type elementType, int dimensions) 
            : base(size)
        {
            ElementType = elementType;
            Dimensions = dimensions;
        }

        public Type ElementType { get; }
        public int Dimensions { get; }

        public override bool Equals(object obj) => 
            obj is ArrayType a && 
            Dimensions == a.Dimensions && 
            ElementType.Equals(a.ElementType);

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => $"[{new string(',', Dimensions - 1)}]{ElementType}";
    }
}
