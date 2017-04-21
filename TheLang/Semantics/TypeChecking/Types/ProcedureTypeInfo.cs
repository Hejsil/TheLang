using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ProcedureTypeInfo : TypeInfo
    {
        public ProcedureTypeInfo(bool isFunction, TypeInfo returnType, IEnumerable<TypeInfo> argumentTypes) 
            : base(PointerSize)
        {
            IsFunction = isFunction;
            ReturnType = returnType;
            ArgumentTypes = argumentTypes;
        }

        public bool IsFunction { get; }
        public TypeInfo ReturnType { get; }
        public IEnumerable<TypeInfo> ArgumentTypes { get; }

        protected bool Equals(ProcedureTypeInfo other) => 
            IsFunction == other.IsFunction && 
            Equals(ReturnType, other.ReturnType) &&
            ArgumentTypes.SequenceEqual(other.ArgumentTypes);

        public override bool Equals(object obj) => 
            base.Equals(obj) && 
            Equals((ProcedureTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .Hash(IsFunction)
                .Hash(ReturnType)
                .HashElements(ArgumentTypes)
                .Value;

        public override string ToString() => 
            IsFunction 
                ? $"func({string.Join(", ", ArgumentTypes)}) -> {ReturnType}" 
                : $"proc({string.Join(", ", ArgumentTypes)}) -> {ReturnType}";
    }
}
