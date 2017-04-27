﻿using TheLang.AST.Bases;
using TheLang.Syntax;

namespace TheLang.AST.Expressions.Operators
{
    public class ArrayPostfix : UnaryNode
    {
        public ArrayPostfix(Position position)
            : base(position)
        { }

        public Node Size { get; set; }
    }
}
