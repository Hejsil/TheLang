using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class PointerType : TypeInfo
    {
        public PointerType(int size, TypeInfo pointingTo, PointerKind kind) 
            : base(size)
        {
            PointingTo = pointingTo;
            Kind = kind;
        }

        public TypeInfo PointingTo { get; }
        public PointerKind Kind { get; }

        public override bool Equals(object obj) => 
            obj is PointerType p && 
            Size == p.Size && 
            Kind == p.Kind && 
            PointingTo.Equals(p.PointingTo);
        
        public override string ToString() => $"Float{Size}";

        public enum PointerKind
        {
            Weak,
            Shared,
            Unique
        }
    }
}
