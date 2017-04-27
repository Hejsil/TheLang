using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class TypeLiteral : UnaryNode
    {
        public TypeLiteral(Position position)
            : base(position)
        { }
        
        public IEnumerable<Node> Values { get; set; }
    }
}
