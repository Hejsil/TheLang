using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public partial class PointerTypeInfo : TypeInfo
    {
        public PointerTypeInfo(TypeInfo pointingTo, PointerKind kind) 
            : base(PointerSize)
        {
            PointingTo = pointingTo;
            Kind = kind;
        }

        public TypeInfo PointingTo { get; }
        public PointerKind Kind { get; }

        protected bool Equals(PointerTypeInfo other) => 
            Equals(PointingTo, other.PointingTo) && 
            Kind == other.Kind;

        public override bool Equals(object obj) => 
            base.Equals(obj) && 
            Equals((PointerTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .Hash(PointingTo)
                .Hash(Kind)
                .Value;

        public override string ToString()
        {
            switch (Kind)
            {
                case PointerKind.Weak:
                    return $"@{PointingTo}";
                case PointerKind.Shared:
                    return $"s@{PointingTo}";
                case PointerKind.Unique:
                    return $"u@{PointingTo}";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
