using System.Collections.Generic;
using TheLang.AST.Bases;

namespace TheLang.AST
{
    public class ASTProgramNode : ASTNode
    {
        public ASTProgramNode() 
            : base(null)
        { }

        public IEnumerable<ASTFileNode> Files { get; set; }
    }
}
