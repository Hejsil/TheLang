using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class TupleTypeInfo : TypeInfo
    {
        public TupleTypeInfo(IEnumerable<TypeInfo> elementTypes) 
            : this(elementTypes.ToArray())
        { }

        public TupleTypeInfo(TypeInfo[] elementTypes)
            : base(elementTypes.Sum(item => item.Size))
        {
            ElementTypes = elementTypes;
        }

        public IEnumerable<TypeInfo> ElementTypes { get; }

        protected bool Equals(TupleTypeInfo other) => ElementTypes.SequenceEqual(other.ElementTypes);

        public override bool Equals(object obj) => 
            base.Equals(obj) && 
            Equals((TupleTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .HashElements(ElementTypes)
                .Value;

        public override string ToString() => $"({string.Join(", ", ElementTypes)})";
    }
}
