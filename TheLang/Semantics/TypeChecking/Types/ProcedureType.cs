using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ProcedureType : TypeInfo
    {
        public ProcedureType(int size, bool isFunction, TypeInfo returnType, IEnumerable<TypeInfo> argumentTypes) 
            : base(size)
        {
            IsFunction = isFunction;
            ReturnType = returnType;
            ArgumentTypes = argumentTypes;
        }

        public bool IsFunction { get; }
        public TypeInfo ReturnType { get; }
        public IEnumerable<TypeInfo> ArgumentTypes { get; }

        public override bool Equals(object obj) => 
            obj is ProcedureType p && 
            Size == p.Size && 
            IsFunction == p.IsFunction && 
            ReturnType.Equals(p.ReturnType) && 
            ArgumentTypes.SequenceEqual(p.ArgumentTypes);
        
        public override string ToString()
        {
            var funcPrefix = IsFunction ? "func" : "proc";
            return $"{funcPrefix}({string.Join(",", ArgumentTypes)})->{ReturnType}";
        }
    }
}
