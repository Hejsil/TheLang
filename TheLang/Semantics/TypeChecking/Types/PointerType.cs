using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class PointerType : Type
    {
        public PointerType(int size, Type pointingTo, PointerKind kind) 
            : base(size)
        {
            PointingTo = pointingTo;
            Kind = kind;
        }

        public Type PointingTo { get; }
        public PointerKind Kind { get; }

        public override bool Equals(object obj) => 
            obj is PointerType p && 
            Size == p.Size && 
            Kind == p.Kind && 
            PointingTo.Equals(p.PointingTo);

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => $"Float{Size}";

        public enum PointerKind
        {
            Weak,
            Shared,
            Unique
        }
    }
}
