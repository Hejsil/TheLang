using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class Indexing : UnaryNode
    {
        public Indexing(Position position)
            : base(position)
        { }

        public Node Argument { get; set; }
    }
}