using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CProgram
    {
        public IEnumerable<CInclude> Includes { get; set; }
        public IEnumerable<CTypedef> Typedefs { get; set; }
        public IEnumerable<CStruct> Structs { get; set; }
        public IEnumerable<CDeclaration> Globals { get; set; }
        public IEnumerable<CFunction> Functions { get; set; }
    }
}
