using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree.Operators.Binaries
{
    public abstract class CBinary : CNode
    {
        public CNode Left { get; set; }
        public CNode Right { get; set; }
    }

    public class CAdd              : CBinary { }
    public class CSub              : CBinary { }
    public class CMul              : CBinary { }
    public class CDiv              : CBinary { }
    public class CAnd              : CBinary { }
    public class COr               : CBinary { }
    public class CGreaterThan      : CBinary { }
    public class CLesserThan       : CBinary { }
    public class CGreaterThanEqual : CBinary { }
    public class CLesserThanEqual  : CBinary { }
    public class CEqualEqual       : CBinary { }
    public class CEqual            : CBinary { }
    public class CDot              : CBinary { }
    public class CModulo           : CBinary { }
    public class CNotEqual         : CBinary { }
    

}
