using System;
using TheLang.Syntax;

namespace TheLang
{
    public class Compiler
    {
        public void ReportError(Position position, string message)
        {
            Console.Error.WriteLine(message);
        }
    }
}
