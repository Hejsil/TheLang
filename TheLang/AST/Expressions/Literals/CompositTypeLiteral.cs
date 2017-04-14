using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class CompositTypeLiteral : Expression
    {
        public CompositTypeLiteral(Position position) 
            : base(position)
        { }

        public Expression Type { get; set; }
        public IEnumerable<Assignment> Values { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
