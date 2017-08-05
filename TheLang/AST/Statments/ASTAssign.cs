using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Statments
{
    public class ASTAssign : ASTBinaryNode
    {
        public ASTAssign(Position position) 
            : base(position)
        { }
    }
}
