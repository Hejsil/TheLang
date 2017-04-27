using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class StructLiteral : UnaryNode
    {
        public StructLiteral(Position position)
            : base(position)
        { }
        
        public IEnumerable<Assignment> Values { get; set; }

        public class Assignment
        {
            public Assignment(Position position)
            {
                Position = position;
            }

            public Position Position { get; }
            public Symbol Left { get; set; }
            public Node Right { get; set; }
        }
    }
}
