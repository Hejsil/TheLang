using System.Collections.Generic;
using System.Linq;
using HashCalculator;

namespace TheLang.Semantics.TypeChecking
{
    public struct TypeInfoStruct
    {
        public TypeInfoStruct(TypeId id, IEnumerable<TypeInfo> children, int size = TypeInfo.NeedToBeInferedSize, string name = null)
        {
            Id = id;
            Size = size;
            Name = name;
            Children = children;
        }

        public TypeInfoStruct(TypeId id, int size = TypeInfo.NeedToBeInferedSize, string name = null)
            : this(id, Enumerable.Empty<TypeInfo>(), size, name)
        { }

        public TypeInfoStruct(TypeId id, params TypeInfo[] children)
            : this(id, (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfoStruct(TypeId id, int size, params TypeInfo[] children)
            : this(id, children, size)
        { }

        public TypeInfoStruct(TypeId id, string name, params TypeInfo[] children)
            : this(id, children, name: name)
        { }

        public TypeInfoStruct(TypeId id, int size, string name, params TypeInfo[] children)
            : this(id, children, size, name)
        { }

        public
        public TypeId Id { get; }
        public int Size { get; }
        public string Name { get; }
        public IEnumerable<TypeInfo> Children { get; }

        public TypeInfo Allocate() => new TypeInfo(this);

        private sealed class TypeInfoStructEqualityComparer : IEqualityComparer<TypeInfoStruct>
        {
            public bool Equals(TypeInfoStruct x, TypeInfoStruct y)
            {
                return x.Id == y.Id &&
                       x.Size == y.Size &&
                       string.Equals(x.Name, y.Name) &&
                       Enumerable.SequenceEqual(x.Children, y.Children);
            }

            public int GetHashCode(TypeInfoStruct obj)
            {
                unchecked
                {
                    var hashCode = (int) obj.Id;
                    hashCode = (hashCode * 397) ^ obj.Size;
                    hashCode = (hashCode * 397) ^ (obj.Name != null ? obj.Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (obj.Children != null ? obj.Children.CalculateHash() : 0);
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<TypeInfoStruct> Comparer { get; } = new TypeInfoStructEqualityComparer();

    }
}