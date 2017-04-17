﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class BooleanType : Type
    {
        public BooleanType(int size) 
            : base(size)
        {
        }

        public override bool Equals(object obj) => 
            obj is BooleanType b && 
            Size == b.Size;

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => $"Bool{Size}";
    }
}
