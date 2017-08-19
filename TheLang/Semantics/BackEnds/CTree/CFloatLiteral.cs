using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeterO.Numbers;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CFloatLiteral : CNode
    {
        public EDecimal Value { get; set; }
    }
}
