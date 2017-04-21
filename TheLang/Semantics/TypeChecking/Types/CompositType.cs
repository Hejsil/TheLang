using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class CompositType : TypeInfo
    {
        public CompositType(int size, IEnumerable<Field> fields) 
            : base(size)
        {
            Fields = fields;
        }

        public IEnumerable<Field> Fields { get; }
        
        public override bool Equals(object obj) => 
            obj is CompositType c && 
            Size == c.Size && 
            Fields.SequenceEqual(c.Fields);

        public override int GetHashCode() => ToString().GetHashCode();
        public override string ToString() => $"struct{{{string.Join(";", Fields)}}}";

        public class Field
        {
            public string Name { get; set; }
            public TypeInfo Type { get; set; }

            public override bool Equals(object obj) => obj is Field f && Name == f.Name && Type.Equals(f.Type);
            public override string ToString() => $"{Name}:{Type}";
        }
    }
}
