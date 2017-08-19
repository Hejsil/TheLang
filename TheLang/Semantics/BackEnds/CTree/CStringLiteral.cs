using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CStringLiteral : CNode
    {
        public string Value { get; set; }
    }
}
