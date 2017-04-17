using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class Symbol : Node
    {
        public Symbol(Position position, string name) 
            : base(position)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
