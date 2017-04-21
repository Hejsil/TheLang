using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ArrayTypeInfo : TypeInfo
    {
        public ArrayTypeInfo(TypeInfo elementType, int dimensions) 
            : base(ArraySize)
        {
            ElementType = elementType;
            Dimensions = dimensions;
        }

        public TypeInfo ElementType { get; }
        public int Dimensions { get; }

        public override bool Equals(object obj) =>
            base.Equals(obj) && 
            Equals((ArrayTypeInfo)obj);

        protected bool Equals(ArrayTypeInfo other) => 
            Equals(ElementType, other.ElementType) && 
            Dimensions == other.Dimensions;

        public override int GetHashCode() => 
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .Hash(ElementType)
                .Hash(Dimensions)
                .Value;

        public override string ToString() => $"[{new string(',', Dimensions - 1)}]{ElementType}";
    }
}
