using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.AST.Statments;

namespace TheLang.AST
{
    public class ProgramNode : Node
    {
        public ProgramNode() 
            : base(null)
        { }

        public IEnumerable<FileNode> Files { get; set; } 
    }
}
