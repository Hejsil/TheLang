using System.Collections.Generic;
using System.Linq;
using OBeautifulCode.Math;
using TheLang.AST.Expressions.Literals;

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

        public TypeId Id { get; }
        public int Size { get; }
        public string Name { get; }
        public IEnumerable<TypeInfo> Children { get; }

        public TypeInfo Allocate() => new TypeInfo(this);

        public static IEqualityComparer<TypeInfoStruct> Comparer { get; } = new TypeInfoStructEqualityComparer();
        private sealed class TypeInfoStructEqualityComparer : IEqualityComparer<TypeInfoStruct>
        {
            public bool Equals(TypeInfoStruct x, TypeInfoStruct y)
            {
                return x.Id == y.Id &&
                       x.Size == y.Size &&
                       string.Equals(x.Name, y.Name) &&
                       x.Children.SequenceEqual(y.Children);
            }

            public int GetHashCode(TypeInfoStruct obj)
            {
                return HashCodeHelper.Initialize()
                    .Hash(obj.Id)
                    .Hash(obj.Name)
                    .HashElements(obj.Children)
                    .Value;
            }
        }
    }
}