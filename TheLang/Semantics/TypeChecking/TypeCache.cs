using System.Collections.Generic;
using TheLang.Semantics.TypeChecking.Types;

namespace TheLang.Semantics.TypeChecking
{
    public class TypeCache
    {
        private readonly Dictionary<int, FloatType> _floatCache = new Dictionary<int, FloatType>();
        private readonly Dictionary<(int, bool), IntegerType> _intCache = new Dictionary<(int, bool), IntegerType>();
        private readonly Dictionary<BaseType, ArrayType> _arrayCache = new Dictionary<BaseType, ArrayType>();
        private readonly Dictionary<BaseType, PointerType> _pointerCache = new Dictionary<BaseType, PointerType>();
        private readonly Dictionary<BaseType, TypeType> _typeCache = new Dictionary<BaseType, TypeType>();
        private readonly BooleanType _boolean = new BooleanType();
        private readonly UnknownType _unknown = new UnknownType();
        private readonly VoidType _void = new VoidType();
        private readonly StringType _string = new StringType();

        public ArrayType GetArray(BaseType elementTypes)
        {
            if (_arrayCache.TryGetValue(elementTypes, out var result)) return result;

            result = new ArrayType(elementTypes);
            _arrayCache.Add(elementTypes, result);
            return result;
        }

        public PointerType GetPointer(BaseType elementTypes)
        {
            if (_pointerCache.TryGetValue(elementTypes, out var result)) return result;

            result = new PointerType(elementTypes);
            _pointerCache.Add(elementTypes, result);
            return result;
        }

        public TypeType GetType(BaseType elementTypes)
        {
            if (_typeCache.TryGetValue(elementTypes, out var result)) return result;

            result = new TypeType(elementTypes);
            _typeCache.Add(elementTypes, result);
            return result;
        }

        public FloatType GetFloat(int size)
        {
            if (_floatCache.TryGetValue(size, out var result)) return result;

            result = new FloatType(size);
            _floatCache.Add(size, result);
            return result;
        }

        public IntegerType GetInt(int size, bool signed)
        {
            if (_intCache.TryGetValue((size, signed), out var result)) return result;

            result = new IntegerType(size, signed);
            _intCache.Add((size, signed), result);
            return result;
        }

        public BooleanType GetBoolean() => _boolean;
        public StringType GetString() => _string;
        public UnknownType GetUnknown() => _unknown;
        public VoidType GetVoid() => _void;
    }
}
