using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTCompilerCall : ASTNode
    {
        public ASTCompilerCall(Position position, string name)
            : base(position)
        {
            Name = name;
        }

        public string Name { get; set; }
        public IEnumerable<ASTNode> Arguments { get; set; }
    }
}
