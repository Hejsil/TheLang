using System;
using System.Collections.Generic;
using System.Text;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class Symbol : Expression
    {
        public Symbol(Position position, string name) 
            : base(position)
        {
            Name = name;
        }

        public string Name { get; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
