using System;
using System.Collections.Generic;
using System.Text;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class ASTCodeBlock : ASTNode
    {
        public ASTCodeBlock(Position position) 
            : base(position)
        { }

        public IEnumerable<ASTNode> Statements { get; set; }
    }
}
