using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST
{
    public class ASTFileNode : ASTNode
    {
        public ASTFileNode(Position position) 
            : base(position)
        { }

        public IEnumerable<ASTNode> Declarations { get; set; }
    }
}
