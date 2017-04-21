using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class TypeType : TypeInfo
    {
        public TypeInfo Type { get; }

        public TypeType(TypeInfo type) 
            : base(64)
        {
            Type = type;
        }

        public override bool Equals(object obj) =>
            obj is TypeType t &&
            Type.Equals(t.Type);

        public override string ToString() => $"Type({Type})";
    }
}
