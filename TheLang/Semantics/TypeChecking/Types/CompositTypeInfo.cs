using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class CompositTypeInfo : TypeInfo
    {
        public CompositTypeInfo(IEnumerable<Field> fields) 
            : this(fields.ToArray())
        { }

        public CompositTypeInfo(Field[] fields)
            : base(fields.Sum(item => item.Type.Size))
        {
            Fields = fields;
        }

        public IEnumerable<Field> Fields { get; }

        protected bool Equals(CompositTypeInfo other) => Fields.SequenceEqual(other.Fields);

        public override bool Equals(object obj) =>
            base.Equals(obj) && 
            Equals((CompositTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .HashElements(Fields)
                .Value;

        public override string ToString() => $"struct{{ {string.Join(" ", Fields)} }}";

        public class Field
        {
            public Field(string name, TypeInfo type)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; }
            public TypeInfo Type { get; }

            protected bool Equals(Field other) => 
                string.Equals(Name, other.Name) && 
                Equals(Type, other.Type);

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Field)obj);
            }

            public override int GetHashCode() =>
                HashCodeHelper.Initialize()
                    .Hash(Name)
                    .Hash(Type)
                    .Value;

            public override string ToString() => $"{Name}: {Type}";
        }
    }
}
