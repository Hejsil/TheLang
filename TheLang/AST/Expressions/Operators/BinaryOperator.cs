using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class BinaryOperator : BinaryNode
    {
        public BinaryOperator(Position position, BinaryOperatorKind kind) 
            : base(position)
        {
            Kind = kind;
        }
        
        public BinaryOperatorKind Kind { get; }
        public int Priority => (int)Kind >> 4;
        public AssociativityKind Associativity => (AssociativityKind)(((int)Kind >> 4) % 2);
    }
}
