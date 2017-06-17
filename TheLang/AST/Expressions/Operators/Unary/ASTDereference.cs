using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators.Unary
{
    public class ASTDereference : ASTUnaryNode
    {
        public ASTDereference(Position position)
            : base(position)
        { }
    }
}