using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTArrayInitializer : ASTUnaryNode
    {
        public ASTArrayInitializer(Position position) 
            : base(position)
        { }
        
        public IEnumerable<ASTNode> Values { get; set; }
    }
}