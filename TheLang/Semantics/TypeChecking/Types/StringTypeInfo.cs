using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class StringTypeInfo : TypeInfo
    {
        public StringTypeInfo() 
            : base(ArraySize)
        { }

        public override string ToString() => "String";
    }
}
