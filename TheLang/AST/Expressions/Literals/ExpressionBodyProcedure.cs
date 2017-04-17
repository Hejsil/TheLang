using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ExpressionBodyProcedure : ProcedureLiteral
    {
        public ExpressionBodyProcedure(Position position, bool isFunction) 
            : base(position, isFunction)
        { }

        public Node Expression { get; set; }
    }
}
