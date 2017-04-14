using System;
using System.Collections.Generic;
using System.Text;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class Call : Expression
    {
        public Call(Position position) 
            : base(position)
        { }

        public Expression Callee { get; set; }
        public IEnumerable<Expression> Arguments { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
