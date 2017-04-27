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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TypeInfo) obj);
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
