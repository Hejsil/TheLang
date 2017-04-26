using System;
using System.Collections.Generic;
using System.IO;
using TheLang.AST;
using TheLang.Semantics.TypeChecking;
using TheLang.Syntax;

namespace TheLang
{
    public class Compiler
    {
        public ProgramNode Tree { get; set; }

        private readonly HashSet<string> _filesInProject = new HashSet<string>();
        private readonly Queue<string> _filesToCompile = new Queue<string>();

        public bool ParseProgram(TextReader reader)
        {
            var files = new List<FileNode>();
            var parser = new Parser(reader, this);

            if (!parser.TryParseFile(out var fileNode))
                return false;

            files.Add(fileNode);

            ParserLoop(files);

            Tree = new ProgramNode { Files = files };

            return true;
        }

        public bool ParseProgram()
        {
            var files = new List<FileNode>();

            if (!ParserLoop(files))
                return false;

            Tree = new ProgramNode { Files = files };

            return true;
        }

        public bool TypeCheck()
        {
            var checker = new TypeChecker(this);
            return checker.Visit(Tree);
        }
        
        public void AddFileToProject(string fileName)
        {
            if (_filesInProject.Add(fileName))
                _filesToCompile.Enqueue(fileName);
        }

        public void ReportError(Position position, string message, string hint)
        {
            Console.Error.WriteLine(
                $"Error at {position.FileName}:{position.Line}:{position.Column}: {message}\n" +
                $"\tHint: {hint}");
        }

        public void ReportError(Position position, string message)
        {
            Console.Error.WriteLine(
                $"Error at {position.FileName}:{position.Line}:{position.Column}: {message}");
        }

        private bool ParserLoop(ICollection<FileNode> result)
        {
            while (_filesToCompile.Count != 0)
            {
                var parser = new Parser(_filesToCompile.Dequeue(), this);

                if (!parser.TryParseFile(out var fileNode))
                    return false;

                result.Add(fileNode);
            }

            return true;
        }
    }
}