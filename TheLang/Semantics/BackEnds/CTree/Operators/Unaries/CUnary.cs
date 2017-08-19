namespace TheLang.Semantics.BackEnds.CTree.Operators.Unaries
{
    public abstract class CUnary : CNode
    {
        public CNode Child { get; set; }
    }

    public class CPointer     : CUnary { }
    public class CReference   : CUnary { }
    public class CDereference : CUnary { }
    public class CPositive    : CUnary { }
    public class CNegative    : CUnary { }
    public class CNot         : CUnary { }
    public class CReturn      : CUnary { }
}
