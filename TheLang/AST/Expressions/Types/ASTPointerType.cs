using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Types
{
    public class ASTPointerType : ASTUnaryNode
    {
        public ASTPointerType(Position position) 
            : base(position)
        { }
    }
}
