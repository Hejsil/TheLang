using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTIntegerLiteral : ASTNode
    {
        public ASTIntegerLiteral(Position position, long value) 
            : base(position)
        {
            Value = value;
        }

        public long Value { get; set; }
    }
}
