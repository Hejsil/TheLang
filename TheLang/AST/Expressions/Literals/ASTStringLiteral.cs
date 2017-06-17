using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Literals
{
    public class ASTStringLiteral : ASTNode
    {
        public ASTStringLiteral(Position position, string value)
            : base(position)
        {
            Value = value;
        }

        public string Value { get; set; }
    }
}
