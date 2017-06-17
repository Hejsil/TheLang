using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTProcedureLiteral : ASTNode
    {
        public ASTProcedureLiteral(Position position, bool isFunction) 
            : base(position)
        {
            IsFunction = isFunction;
        }

        public bool IsFunction { get; set; }
        public ASTNode Return { get; set; }
        public IEnumerable<ASTDeclaration> Arguments { get; set; }
        public ASTCodeBlock Block { get; set; }
    }
}
