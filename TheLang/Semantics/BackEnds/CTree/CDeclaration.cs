﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.BackEnds.CTree
{
    public class CDeclaration : CNode
    {
        public CNode Type { get; set; }
        public string Name { get; set; }
        public CNode Value { get; set; }
    }
}
