using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTLambda : ASTNode
    {
        public ASTLambda(Position position) 
            : base(position)
        { }
        
        public ASTNode Return { get; set; }
        public IEnumerable<Argument> Arguments { get; set; }
        public ASTCodeBlock Block { get; set; }

        public class Argument : ASTNode
        {
            public Argument(Position position) 
                : base(position)
            { }

            public ASTSymbol Symbol { get; set; }
            public ASTNode Type { get; set; }
            public ASTNode DefaultValue { get; set; }
        }
    }
}
