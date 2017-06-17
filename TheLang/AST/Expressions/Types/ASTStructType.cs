using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Types
{
    public class ASTStructType : ASTNode
    {
        public ASTStructType(Position position)
            : base(position)
        { }

        public IEnumerable<ASTDeclaration> Fields { get; set; }
    }
}