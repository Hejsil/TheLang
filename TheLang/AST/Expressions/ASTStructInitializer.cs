using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTStructInitializer : ASTUnaryNode
    {
        public ASTStructInitializer(Position position)
            : base(position)
        { }
        
        public IEnumerable<ASTEqual> Values { get; set; }
    }
}
