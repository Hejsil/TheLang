using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class ASTDeclaration : ASTNode
    {
        public ASTDeclaration(Position position) 
            : base(position)
        { }

        public string Name { get; set; }
        public ASTNode DeclaredType { get; set; }
    }
}
