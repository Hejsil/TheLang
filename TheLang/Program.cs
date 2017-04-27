using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheLang
{
    public class Program
    {
        public static void Main()
        {
            var compiler = new Compiler();

            using (var str = new StringReader("test :: 1 + 2"))
                compiler.ParseProgram(str);

            compiler.TypeCheck();
        }
    }
}
