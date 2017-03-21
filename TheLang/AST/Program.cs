using System.Collections.Generic;
using TheLang.AST.Statments;

namespace TheLang.AST
{
    public class Program : Node
    {
        public Program(IEnumerable<Node> declarations) 
            : base(null)
        {
            Declarations = declarations;
        }

        public IEnumerable<Node> Declarations { get; set; } 

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
