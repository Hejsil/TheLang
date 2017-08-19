using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ProcedureType : BaseType
    {
        protected bool Equals(ProcedureType other) 
            => base.Equals(other) && Equals(Return, other.Return) && Arguments.SequenceEqual(other.Arguments);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ProcedureType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Return != null ? Return.GetHashCode() : 0);
                return Arguments.Aggregate(hashCode, (current, arg) => (current * 397) ^ (arg != null ? arg.GetHashCode() : 0));
            }
        }

        public override string ToString() => $"proc({string.Join(", ", Arguments)})->{Return}";

        public ProcedureType(IEnumerable<Argument> arguments, BaseType returnType)
            : base(64)
        {
            Arguments = arguments;
            Return = returnType;
        }

        public BaseType Return { get; }
        public IEnumerable<Argument> Arguments { get; }

        public class Argument
        {
            protected bool Equals(Argument other) => Equals(Type, other.Type);
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == GetType() && Equals((Argument) obj);
            }

            public override int GetHashCode() => Type != null ? Type.GetHashCode() : 0;
            public override string ToString() => Name == null ? $"{Type}" : $"{Name}: {Type}";

            public Argument(string name, BaseType type)
            {
                Name = name;
                Type = type;
            }

            public string Name { get; }
            public BaseType Type { get; }
        }
    }
}
