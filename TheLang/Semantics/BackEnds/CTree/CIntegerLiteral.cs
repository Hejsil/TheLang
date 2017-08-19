using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeterO.Numbers;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CIntegerLiteral : CNode
    {
        public EInteger Value { get; set; }
    }
}
