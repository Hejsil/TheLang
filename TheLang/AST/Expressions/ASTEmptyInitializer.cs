using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTEmptyInitializer : ASTUnaryNode
    {
        public ASTEmptyInitializer(Position position) 
            : base(position)
        { }
    }
}