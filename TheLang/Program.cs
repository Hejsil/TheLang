using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TheLang
{
    public class Program
    {
        public static void Main()
        {
            var type = typeof(Compiler);

            new Compiler().Compile("Examples\\HelloWorld.tl");
            new Compiler().Compile("Examples\\VariablesAndConstants.tl");

            Console.ReadKey();
        }
    }
}
