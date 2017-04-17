using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class BlockBodyProcedure : ProcedureLiteral
    {
        public BlockBodyProcedure(Position position, bool isFunction) 
            : base(position, isFunction)
        { }

        public CodeBlock Block { get; set; }
    }
}
