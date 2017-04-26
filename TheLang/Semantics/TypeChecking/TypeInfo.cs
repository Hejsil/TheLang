using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class TypeInfo
    {
        public const int Bit64 = 64;
        public const int NeedToBeInferedSize = -1;
        public const int PointerSize = Bit64;
        public const int Int64Size = Bit64;
        public const int Float64Size = Bit64;
        public const int ArraySize = PointerSize + Int64Size;

        public TypeInfo(Kind id, int size = NeedToBeInferedSize, string name = null, IEnumerable<TypeInfo> children = null)
        {
            // TODO: Infer size, if not provided
            Id = id;
            Size = size;
            Name = name;
            Children = children;
        }

        public TypeInfo(Kind id, params TypeInfo[] children)
            : this(id, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfo(Kind id, int size, params TypeInfo[] children)
            : this(id, size, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfo(Kind id, int size, string name = null, params TypeInfo[] children)
            : this(id, size, name, (IEnumerable<TypeInfo>)children)
        { }

        private int _size;

        public Kind Id { get; }
        public int Size { get; }
        public string Name { get; }
        public IEnumerable<TypeInfo> Children { get; }

        protected bool Equals(TypeInfo other) =>
            Id == other.Id &&
            ReferenceEquals(Children, other.Children) &&
            Children.SequenceEqual(other.Children);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TypeInfo)obj);
        }

        public override int GetHashCode() => 
            HashCodeHelper.Initialize()
                .Hash(Id)
                .Hash(Name)
                .HashCollection(Children)
                .Value;

        public enum Kind
        {
            Array,
            Bool,
            Composit,
            Field,
            Constant,
            Float,
            Integer,
            Nothing,
            Pointer,
            UniquePointer,
            String,
            Tuple,
            Type
        }
    }
}
