using System.Numerics;
using PeterO.Numbers;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTIntegerLiteral : ASTNode
    {
        public ASTIntegerLiteral(Position position, EInteger value) 
            : base(position)
        {
            Value = value;
        }

        public EInteger Value { get; set; }
    }
}
