using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLang.Util
{
    public static class Generic
    {
        public static T1 Max<T1>(params T1[] things) => Max(a => a, things);
        public static T1 Max<T1>(Comparer<T1> comparer, params T1[] things) => Max(comparer, a => a, things);
        public static T1 Max<T1, T2>(Func<T1, T2> selector, params T1[] things) => Max(Comparer<T2>.Default, selector, things);

        public static T1 Max<T1, T2>(Comparer<T2> comparer, Func<T1, T2> selector, params T1[] things)
        {
            var max = things.FirstOrDefault();
            foreach (var thing in things)
            {
                if (comparer.Compare(selector(thing), selector(max)) > 0)
                    max = thing;
            }

            return max;
        }
    }
}
