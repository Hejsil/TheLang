using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Variable : Declaration
    {
        public Variable(Position position, bool isConstant)
            : base(position)
        {
            IsConstant = isConstant;
        }
       
        public Node Value { get; set; }
        public bool IsConstant { get; set; }
    }
}
