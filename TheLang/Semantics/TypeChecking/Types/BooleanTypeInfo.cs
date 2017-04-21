using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class BooleanTypeInfo : TypeInfo
    {
        public BooleanTypeInfo(int size) 
            : base(size)
        { }

        public override string ToString() => $"Bool{Size}";
    }
}
