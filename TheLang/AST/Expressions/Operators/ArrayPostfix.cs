using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class ArrayPostfix : UnaryNode
    {
        public ArrayPostfix(Position position, int dimensions) 
            : base(position)
        {
            Dimensions = dimensions;
        }

        public int Dimensions { get; }
    }
}
