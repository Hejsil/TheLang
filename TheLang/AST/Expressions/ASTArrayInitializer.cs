using System.Collections.Generic;
using TheLang.AST.Bases;

namespace TheLang.Syntax
{
    public class ASTArrayInitializer : ASTUnaryNode
    {
        public ASTArrayInitializer(Position position) 
            : base(position)
        { }
        
        public IEnumerable<ASTNode> Values { get; set; }
    }
}