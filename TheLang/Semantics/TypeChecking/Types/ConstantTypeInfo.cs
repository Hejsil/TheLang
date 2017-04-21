using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBeautifulCode.Math;

namespace TheLang.Semantics.TypeChecking.Types
{
    public class ConstantTypeInfo : TypeInfo
    {
        protected ConstantTypeInfo(TypeInfo child)
            : base(child.Size)
        {
            ChildTypeInfo = child;
        }

        public TypeInfo ChildTypeInfo { get; }

        protected bool Equals(ConstantTypeInfo other) => Equals(ChildTypeInfo, other.ChildTypeInfo);

        public override string ToString() => $"Const {ChildTypeInfo}";

        public override bool Equals(object obj) => 
            base.Equals(obj) && 
            Equals((ConstantTypeInfo)obj);

        public override int GetHashCode() =>
            HashCodeHelper.Initialize()
                .Hash(base.GetHashCode())
                .Hash(ChildTypeInfo)
                .Value;
    }
}
