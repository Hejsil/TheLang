using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Expressions;
using TheLang.AST.Types;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Assignment : Node
    {
        public Assignment(Position position) 
            : base(position)
        { }

        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
