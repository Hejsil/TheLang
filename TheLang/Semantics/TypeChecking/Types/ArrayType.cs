using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ArrayType : TypeInfo
    {
        public ArrayType(int size, TypeInfo elementType, int dimensions) 
            : base(size)
        {
            ElementType = elementType;
            Dimensions = dimensions;
        }

        public TypeInfo ElementType { get; }
        public int Dimensions { get; }

        public override bool Equals(object obj) => 
            obj is ArrayType a && 
            Dimensions == a.Dimensions && 
            ElementType.Equals(a.ElementType);
        
        public override string ToString() => $"[{new string(',', Dimensions - 1)}]{ElementType}";
    }
}
