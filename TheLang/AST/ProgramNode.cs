using System.Collections.Generic;
using TheLang.AST.Bases;

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
