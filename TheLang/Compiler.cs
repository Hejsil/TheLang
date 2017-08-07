using System;
using System.Collections.Generic;
using System.IO;
using TheLang.AST;
using TheLang.Semantics.TypeChecking;
using TheLang.Semantics.TypeChecking.Types;
using TheLang.Syntax;

namespace TheLang
{
    public class Compiler
    {
        public class BuiltIn
        {
            public BuiltIn(Kind identifier, ProcedureType type)
            {
                Identifier = identifier;
                Type = type;
            }

            public enum Kind
            {
                Print
            }

            public Kind Identifier { get; }
            public ProcedureType Type { get; }
        }

        public ASTProgramNode Tree { get; set; }

        public Dictionary<string, BuiltIn> Functions { get; } = new Dictionary<string, BuiltIn>();

        public TypeCache TypeCache { get; } = new TypeCache();

        private readonly HashSet<string> _filesInProject = new HashSet<string>();
        private readonly Queue<string> _filesToCompile = new Queue<string>();

        public Compiler()
        {
            Functions.Add("print",
                new BuiltIn(
                    BuiltIn.Kind.Print,
                    new ProcedureType(null,
                        new []
                        {
                            new ProcedureType.Argument("format_string", TypeCache.GetString()),
                            // TODO: Support argument formatting
                        },
                        TypeCache.GetVoid()
                    )
                )
            );
        }

        public bool ParseProgram(TextReader reader)
        {
            var files = new List<ASTFileNode>();
            var parser = new Parser(reader, this);

            var fileNode = parser.ParseFile();
            if (fileNode == null)
                return false;

            files.Add(fileNode);

            ParserLoop(files);

            Tree = new ASTProgramNode { Files = files };

            return true;
        }

        public bool ParseProgram()
        {
            var files = new List<ASTFileNode>();

            if (!ParserLoop(files))
                return false;

            Tree = new ASTProgramNode { Files = files };

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

        public void ReportError(Position position, string sender, string message, string hint)
        {
            Console.Error.WriteLine(
                $"Error at {position.FileName}:{position.Line}:{position.Column}: {sender}: {message}\n" +
                $"\tHint: {hint}");
        }

        public void ReportError(Position position, string sender, string message)
        {
            Console.Error.WriteLine(
                $"Error at {position.FileName}:{position.Line}:{position.Column}: {sender}: {message}");
        }

        private bool ParserLoop(ICollection<ASTFileNode> result)
        {
            while (_filesToCompile.Count != 0)
            {
                var parser = new Parser(_filesToCompile.Dequeue(), this);

                var fileNode = parser.ParseFile();
                if (fileNode == null)
                    return false;

                result.Add(fileNode);
            }

            return true;
        }
    }
}