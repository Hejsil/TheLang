using System;
using System.Collections.Generic;
using System.Linq;

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

        public TypeInfo(TypeInfoStruct value) => Data = value;

        public bool IsImplicitlyConvertibleTo(TypeInfo type)
        {
            if (Id == TypeId.UNumber || Id == TypeId.Number)
            {
                switch (type.Id)
                {
                    case TypeId.UInteger:
                        return Id == TypeId.UNumber;
                    case TypeId.UNumber:
                        return Id == TypeId.UNumber;
                    case TypeId.Number:
                    case TypeId.Integer:
                    case TypeId.Float:
                        return true;

                    default:
                        return false;
                }
            }

            if (Id != type.Id)
                return false;

            if (Size == type.Size)
                return true;

            switch (Id)
            {
                case TypeId.UInteger:
                case TypeId.Integer:
                case TypeId.Float:
                    return Size <= type.Size;
            }

            return false;
        }

        public override string ToString()
        {
            switch (Id)
            {
                case TypeId.Array:
                    return $"[]{Children.First()}";
                case TypeId.Bool:
                    return $"Bool{Size}";
                case TypeId.Struct:
                    return Name;
                case TypeId.Field:
                    return $"{Name}: {Children.First()}";
                case TypeId.Float:
                    return $"Float{Size}";
                case TypeId.Number:
                    return "SNumber";
                case TypeId.UNumber:
                    return "UNumber";
                case TypeId.Integer:
                    return $"Int{Size}";
                case TypeId.UInteger:
                    return $"UInt{Size}";
                case TypeId.Nothing:
                    return "Nothing";
                case TypeId.Pointer:
                    return $"@{Children.First()}";
                case TypeId.UniquePointer:
                    return $"u@{Children.First()}";
                case TypeId.String:
                    return "String";
                case TypeId.Procedure:
                    return $"proc({string.Join(", ", Children.Take(Children.Count() - 1))}) {Children.Last()}";
                case TypeId.Function:
                    return $"func({string.Join(", ", Children.Take(Children.Count() - 1))}) {Children.Last()}";
                case TypeId.Tuple:
                    return $"({string.Join(", ", Children)})";
                case TypeId.Type:
                    return "Type";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
