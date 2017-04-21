using System;
using System.Collections.Generic;
using System.IO;
using TheLang.AST.Bases;
using TheLang.Semantics.TypeChecking.Types;
using TheLang.Syntax;

namespace TheLang
{
    public class Compiler
    {
        private readonly HashSet<string> _filesInProject = new HashSet<string>();
        private readonly Queue<string> _filesToCompile = new Queue<string>();

        private Compiler()
        { }

        public void AddFileToProject(string fileName)
        {
            if (!_filesInProject.Add(fileName))
                _filesToCompile.Enqueue(fileName);
        }


        private bool Compile()
        {


            return true;
        }

        public static bool Compile(string fileName)
        {
            var compiler = new Compiler();

            compiler.AddFileToProject(fileName);
            return compiler.Compile();
        }

        public void ReportError(Position position, string message)
        {
            Console.Error.WriteLine($"Error at {position.FileName}:{position.Line}:{position.Column}: {message}");
        }
    }
}