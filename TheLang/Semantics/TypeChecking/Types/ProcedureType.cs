using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ProcedureType : Type
    {
        public ProcedureType(int size, bool isFunction, Type returnType, IEnumerable<Type> argumentTypes) 
            : base(size)
        {
            IsFunction = isFunction;
            ReturnType = returnType;
            ArgumentTypes = argumentTypes;
        }

        public bool IsFunction { get; }
        public Type ReturnType { get; }
        public IEnumerable<Type> ArgumentTypes { get; }

        public override bool Equals(object obj) => 
            obj is ProcedureType p && 
            Size == p.Size && 
            IsFunction == p.IsFunction && 
            ReturnType.Equals(p.ReturnType) && 
            ArgumentTypes.SequenceEqual(p.ArgumentTypes);

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString()
        {
            var funcPrefix = IsFunction ? "func" : "proc";
            return $"{funcPrefix}({string.Join(",", ArgumentTypes)})->{ReturnType}";
        }
    }
}
