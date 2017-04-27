using System.Collections.Generic;
using System.Linq;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking
{
    public struct TypeInfoStruct
    {

        public TypeInfoStruct(TypeId id, int size = TypeInfo.NeedToBeInferedSize, string name = null, IEnumerable<TypeInfo> children = null)
        {
            // TODO: Infer size, if not provided
            Id = id;
            Size = size;
            Name = name;
            Children = children;
        }

        public TypeInfoStruct(TypeId id, params TypeInfo[] children)
            : this(id, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfoStruct(TypeId id, int size, params TypeInfo[] children)
            : this(id, size, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfoStruct(TypeId id, string name, params TypeInfo[] children)
            : this(id, name: name, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfoStruct(TypeId id, int size, string name, params TypeInfo[] children)
            : this(id, size, name, (IEnumerable<TypeInfo>)children)
        { }

        public TypeId Id { get; }
        public int Size { get; }
        public string Name { get; }
        public IEnumerable<TypeInfo> Children { get; }

        public TypeInfo Allocate() => new TypeInfo(this);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(Id)
                .Hash(Name)
                .HashElements(Children)
                .Value;

        public bool Equals(ref TypeInfoStruct other) =>
            Id == other.Id &&
            Size == other.Size &&
            string.Equals(Name, other.Name) &&
            Children.SequenceEqual(other.Children);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypeInfoStruct && Equals((TypeInfoStruct) obj);
        }

        public static bool operator ==(TypeInfoStruct type1, TypeInfoStruct type2) => type1.Equals(type2);
        public static bool operator !=(TypeInfoStruct type1, TypeInfoStruct type2) => !type1.Equals(type2);

        public static IEqualityComparer<TypeInfoStruct> Comparer { get; } = new TypeInfoStructEqualityComparer();
        private sealed class TypeInfoStructEqualityComparer : IEqualityComparer<TypeInfoStruct>
        {
            public bool Equals(TypeInfoStruct x, TypeInfoStruct y) => x == y;
            public int GetHashCode(TypeInfoStruct obj) => obj.GetHashCode();
        }
    }
}