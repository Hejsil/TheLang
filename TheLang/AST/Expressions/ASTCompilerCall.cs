using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions
{
    public class ASTCompilerCall : ASTNode
    {
        public ASTCompilerCall(Position position, Compiler.BuiltIn procedure)
            : base(position) => Procedure = procedure;

        public Compiler.BuiltIn Procedure { get; set; }
        public IEnumerable<ASTNode> Arguments { get; set; }
    }
}
