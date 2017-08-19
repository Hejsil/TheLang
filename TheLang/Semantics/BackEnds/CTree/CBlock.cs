using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CBlock : CNode
    {
        public IEnumerable<CNode> Statements { get; set; }
    }
}
