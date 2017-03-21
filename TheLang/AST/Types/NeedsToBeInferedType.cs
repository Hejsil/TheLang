using System;
using System.Collections.Generic;
using System.Text;
using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public class NeedsToBeInferedType : TypeNode
    {
        public NeedsToBeInferedType(Position position) 
            : base(position)
        { }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
