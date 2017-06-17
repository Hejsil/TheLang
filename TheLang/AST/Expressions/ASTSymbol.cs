using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTSymbol : ASTNode
    {
        public ASTSymbol(Position position, string name) 
            : base(position) => Name = name;

        public string Name { get; }
    }
}
