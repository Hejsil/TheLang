using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class TypedProcedureLiteral : Node
    {
        public TypedProcedureLiteral(Position position, bool isFunction) 
            : base(position)
        {
            IsFunction = isFunction;
        }

        public bool IsFunction { get; set; }
        public Node Return { get; set; }
        public IEnumerable<Declaration> Arguments { get; set; }
        public CodeBlock Block { get; set; }
    }
}
