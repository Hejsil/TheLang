using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTCall : ASTUnaryNode
    {
        public ASTCall(Position position) 
            : base(position)
        { }
        
        public IEnumerable<ASTNode> Arguments { get; set; }
    }
}
