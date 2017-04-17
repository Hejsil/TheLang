using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class CodeBlock : Node
    {
        public CodeBlock(Position position) 
            : base(position)
        { }

        public IEnumerable<Node> Statements { get; set; }
    }
}
