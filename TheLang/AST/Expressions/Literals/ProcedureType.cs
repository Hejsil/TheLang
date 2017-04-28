using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ProcedureType : Node
    {
        public ProcedureType(Position position, bool isFunction)
            : base(position)
        {
            IsFunction = isFunction;
        }
        
        public bool IsFunction { get; set; }
        public IEnumerable<Node> Arguments { get; set; }
    }
}
