using System.Collections.Generic;
using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST
{
    public class FileNode : Node
    {
        public FileNode(Position position) 
            : base(position)
        { }

        public IEnumerable<Node> Declarations { get; set; }
    }
}
