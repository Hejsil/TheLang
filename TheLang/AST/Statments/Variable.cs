using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Variable : Node
    {
        public Variable(Position position, bool isConstant)
            : base(position)
        {
            IsConstant = isConstant;
        }

        public Declaration Declaration { get; set; }
        public Node Value { get; set; }
        public bool IsConstant { get; set; }
    }
}
