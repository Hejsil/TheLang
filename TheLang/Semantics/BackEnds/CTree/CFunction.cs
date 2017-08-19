using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CFunction : CNode
    {
        public CNode Return { get; set; }
        public string Name { get; set; }
        public IEnumerable<Argument> Arguments { get; set; }
        public CBlock Block { get; set; }
        
        public class Argument
        {
            public CNode Type { get; set; }
            public string Name { get; set; }
        }
    }
}
