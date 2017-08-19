using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CStruct : CNode
    {
        public string Name { get; set; }
        public IEnumerable<Field> Enumerable { get; set; }

        public class Field
        {
            public CNode Type { get; set; }
            public string Name { get; set; }
        }
    }
}
