using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class Declaration : Node
    {
        public Declaration(Position position) 
            : base(position)
        { }

        public Symbol Name { get; set; }
        public Node DeclaredType { get; set; }
    }
}
