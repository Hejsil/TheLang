using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class CompositTypeLiteral : UnaryNode
    {
        public CompositTypeLiteral(Position position) 
            : base(position)
        { }
        
        public IEnumerable<BinaryOperator> Values { get; set; }
    }
}
