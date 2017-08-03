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

        public IEnumerable<Field> Fields { get; set; }

        public class Field : ASTNode
        {
            public Field(Position position) 
                : base(position)
            { }

            public IEnumerable<ASTSymbol> Symbols { get; set; }
            public ASTNode Type { get; set; }
            public ASTNode DefaultValue { get; set; }
        }
    }
}