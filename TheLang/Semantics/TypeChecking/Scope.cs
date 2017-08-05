using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLang.Semantics.TypeChecking.Types;

namespace TheLang.Semantics.TypeChecking
{
    public class Scope
    {
        public Scope Parent { get; set; }
        private Dictionary<string, BaseType> Symbols { get; } = new Dictionary<string, BaseType>();

        public bool TryGetTypeOf(string symbol, out BaseType result)
        {
            if (Symbols.TryGetValue(symbol, out result)) return true;
            if (Parent == null) return false;
            return Parent.TryGetTypeOf(symbol, out result);
        }

        public bool TryAddSymbol(string symbol, BaseType type)
        {
            try
            {
                Symbols.Add(symbol, type);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
