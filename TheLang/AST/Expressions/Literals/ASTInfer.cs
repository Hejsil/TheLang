using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTInfer : ASTNode
    {
        public ASTInfer()
            : base(new Position("", 0, 0))
        { }
    }
}