using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Types
{
    public class ASTProcedureType : ASTNode
    {
        public ASTProcedureType(Position position, bool isFunction)
            : base(position)
        {
            IsFunction = isFunction;
        }
        
        public bool IsFunction { get; set; }
        public IEnumerable<ASTNode> Arguments { get; set; }
    }
}
