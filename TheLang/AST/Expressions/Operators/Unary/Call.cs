using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class Call : UnaryNode
    {
        public Call(Position position) 
            : base(position)
        { }
        
        public IEnumerable<Node> Arguments { get; set; }
    }
}
