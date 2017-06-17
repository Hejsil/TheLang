using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class ASTVariable : ASTDeclaration
    {
        public ASTVariable(Position position)
            : base(position)
        { }
       
        public ASTNode Value { get; set; }
        public bool IsConstant { get; set; }
    }
}
