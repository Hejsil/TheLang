using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TheLang.Semantics.TypeChecking.Types;

namespace TheLang
{
    public class Program
    {
        private const string Test = @"
const main := proc(args: []String) {
    #print(""Hello World!"")
}
";

        public static void Main()
        {
            var compiler = new Compiler();

            using (var str = new StringReader(Test))
                compiler.ParseProgram(str);

            var typePassed = compiler.TypeCheck();
        }
    }
}
