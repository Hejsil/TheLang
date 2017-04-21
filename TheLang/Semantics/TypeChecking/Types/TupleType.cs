using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class TupleType : TypeInfo
    {
        public TupleType(int size, IEnumerable<TypeInfo> elementTypes) 
            : base(size)
        {
            ElementTypes = elementTypes;
        }
        
        public IEnumerable<TypeInfo> ElementTypes { get; }

        public override bool Equals(object obj) =>
            obj is TupleType t &&
            Size == t.Size &&
            ElementTypes.SequenceEqual(t.ElementTypes);
        
        public override string ToString() => $"({string.Join(",", ElementTypes)})";
    }
}
