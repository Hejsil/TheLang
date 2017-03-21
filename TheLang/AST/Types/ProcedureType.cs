using System.Collections.Generic;
using TheLang.Syntax;

namespace TheLang.AST.Types
{
    public class ProcedureType : TypeNode
    {
        public ProcedureType(Position position, TypeNode returnType, IEnumerable<TypeNode> argumentTypes, bool isFunction) 
            : base(position)
        {
            ReturnType = returnType;
            ArgumentTypes = argumentTypes;
            IsFunction = isFunction;
        }

        public TypeNode ReturnType { get; }
        public IEnumerable<TypeNode> ArgumentTypes { get; }
        public bool IsFunction { get; }

        public override bool Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
