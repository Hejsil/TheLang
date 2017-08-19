using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CCall : CNode
    {
        public CNode Callee { get; set; }
        public IEnumerable<CNode> Arguments { get; set; }
    }
}
