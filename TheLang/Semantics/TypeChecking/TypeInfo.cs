using System.Collections.Generic;
using System.Linq;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeInfo
    {
        public const int Bit8 = 8;
        public const int Bit16 = 16;
        public const int Bit32 = 32;
        public const int Bit64 = 64;
        public const int NeedToBeInferedSize = -1;


        public TypeInfoStruct Data { get; }
        public TypeId Id => Data.Id;
        public int Size => Data.Size;
        public string Name => Data.Name;
        public IEnumerable<TypeInfo> Children => Data.Children;

        public TypeInfo(TypeInfoStruct value)
        {
            Data = value;
        }

        public TypeInfo(TypeId id, int size = NeedToBeInferedSize, string name = null, IEnumerable<TypeInfo> children = null)
            : this(new TypeInfoStruct(id, size, name, children))
        { }

        public TypeInfo(TypeId id, params TypeInfo[] children)
            : this(id, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfo(TypeId id, int size, params TypeInfo[] children)
            : this(id, size, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfo(TypeId id, string name, params TypeInfo[] children)
            : this(id, name: name, children: (IEnumerable<TypeInfo>)children)
        { }

        public TypeInfo(TypeId id, int size, string name, params TypeInfo[] children)
            : this(id, size, name, (IEnumerable<TypeInfo>)children)
        { }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((TypeInfo) obj);
        }

        protected bool Equals(TypeInfo other) => Data.Equals(other.Data);
        public override int GetHashCode() => Data.GetHashCode();

        public bool IsImplicitlyConvertibleTo(TypeInfo type)
        {
            if (Id != type.Id)
                return false;

            if (Size == type.Size)
                return true;

            switch (Id)
            {
                case TypeId.UInteger:
                case TypeId.Integer:
                case TypeId.Float:
                    if (Id != type.Id)
                        return false;

                    return Size <= type.Size;
            }

            return false;
        }
    }
}
