using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class Nothing : Type
    {

        public Nothing() 
            : base(0)
        {
        }


        public override bool Equals(object obj) => obj is Nothing;
        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => "Nothing";
    }
}
