using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTStructInitializer : ASTUnaryNode
    {
        public ASTStructInitializer(Position position)
            : base(position)
        { }
        
        public IEnumerable<ASTNode> Values { get; set; }
    }
}
