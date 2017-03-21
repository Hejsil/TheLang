using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public abstract class Expression : Node
    {
        protected Expression(Position position) 
            : base(position)
        { }
    }
}
