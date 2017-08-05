using PeterO.Numbers;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTFloatLiteral : ASTNode
    {
        public ASTFloatLiteral(Position position, EDecimal value) 
            : base(position)
        {
            Value = value;
        }

        public EDecimal Value { get; set; }
    }
}
