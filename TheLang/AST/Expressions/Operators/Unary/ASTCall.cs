using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTCall : ASTUnaryNode
    {
        public ASTCall(Position position) 
            : base(position)
        { }
        
        public IEnumerable<ASTNode> Arguments { get; set; }
    }
}
