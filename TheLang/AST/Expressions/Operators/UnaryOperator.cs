using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class UnaryOperator : UnaryNode
    {
        public UnaryOperator(Position position, UnaryOperatorKind kind) 
            : base(position)
        {
            Kind = kind;
        }
        
        public UnaryOperatorKind Kind { get; set; }
    }
}
