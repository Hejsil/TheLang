using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class TupleLiteral : Node
    {
        public TupleLiteral(Position position) 
            : base(position)
        { }

        public IEnumerable<Node> Items { get; set; }
    }
}
