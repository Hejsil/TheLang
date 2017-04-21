using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class NothingTypeInfo : TypeInfo
    {
        public NothingTypeInfo() 
            : base(0)
        { }

        public override string ToString() => "Nothing";
    }
}
