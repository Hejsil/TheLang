using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class Infer : Node
    {
        public Infer()
            : base(new Position("", 0, 0))
        { }
    }
}