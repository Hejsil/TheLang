using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Statments;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class StructType : Node
    {
        public StructType(Position position)
            : base(position)
        { }

        public IEnumerable<Declaration> Fields { get; set; }
    }
}