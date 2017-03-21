using System;
using System.Collections.Generic;
using System.IO;
using TheLang.AST;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;
using TheLang.AST.Types;

namespace TheLang.Syntax
{
    public class Parser
    {
        private HashSet<string> _filesInProject;
        private Queue<string> _filesToCompile;
        private Scanner _currentScanner;

        private readonly Compiler _compiler;

        public Parser(Compiler compiler)
        {
            _compiler = compiler;
        }

        public bool TryParseProgram(TextReader stream, out Program result)
        {
            InitParser(stream);
            return TryParseProgram(out result);
        }

        public bool TryParseProgram(string fileName, out Program result)
        {
            InitParser(fileName);
            return TryParseProgram(out result);
        }

        public bool TryParseDeclaration(TextReader stream, out Declaration result)
        {
            InitParser(stream);
            return TryParseDeclaration(out result);
        }

        public bool TryParseDeclaration(string fileName, out Declaration result)
        {
            InitParser(fileName);
            return TryParseDeclaration(out result);
        }

        public bool TryParseType(TextReader stream, out TypeNode result)
        {
            InitParser(stream);
            return TryParseType(out result);
        }

        public bool TryParseType(string fileName, out TypeNode result)
        {
            InitParser(fileName);
            return TryParseType(out result);
        }

        public bool TryParseExpression(TextReader stream, out Expression result)
        {
            InitParser(stream);
            return TryParseExpression(out result);
        }

        public bool TryParseExpression(string fileName, out Expression result)
        {
            InitParser(fileName);
            return TryParseExpression(out result);
        }

        private bool TryParseProgram(out Program result)
        {
            result = null;

            var running = true;
            var declarations = new List<Node>();

            do
            {
                var next = PeekToken();
                switch (next.Kind)
                {
                    // If EndOfFile, then we need to load in a new scanner
                    case TokenKind.EndOfFile:
                        do
                        {
                            if (_filesToCompile.Count == 0)
                            {
                                running = false;
                                break;
                            }

                            _currentScanner = new Scanner(_filesToCompile.Dequeue());
                        } while (PeekIs(TokenKind.EndOfFile));
                        break;

                    // Else, the only this allowed in the grobal scope, for now, is a declaration
                    case TokenKind.Identifier:
                        if (TryParseDeclaration(out Declaration declaration))
                        {
                            next = PeekToken();
                            switch (next.Kind)
                            {
                                case TokenKind.Equal:
                                case TokenKind.Colon:
                                    if (TryParseExpression(out Expression expression))
                                    {
                                        declarations.Add(new Variable(next.Position, declaration, expression,
                                            next.Kind == TokenKind.Colon));
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    
                                    break;
                                default:
                                    declarations.Add(declaration);
                                    break;
                            }
                        }
                        else
                        {
                            return false;
                        }

                        break;
                    default:
                        // TODO: better error message
                        _compiler.ReportError(next.Position, "Error");
                        return false;
                }
            } while (running);

            result = new Program(declarations);
            return true;
        }

        /// <summary>
        /// 
        /// 
        /// identifier : maybe(type) = something
        /// identifier : maybe(type) : something
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryParseDeclaration(out Declaration result)
        {
            TypeNode declarationType;

            result = null;
            
            if (!TryEatToken(TokenKind.Identifier, out Token identifier))
            {
                // TODO: better error message
                _compiler.ReportError(identifier.Position, "Error");
                return false;
            }

            if (!TryEatToken(TokenKind.Colon, out Token colon))
            {
                // TODO: better error message
                _compiler.ReportError(colon.Position, "Error");
                return false;
            }

            var next = PeekToken();
            if (next.Kind != TokenKind.Colon && next.Kind != TokenKind.Equal)
            {
                if (!TryParseType(out declarationType))
                    return false;
            }
            else
            {
                declarationType = new NeedsToBeInferedType(next.Position);
            }
            
            result = new Declaration(identifier.Position, identifier.Value, declarationType);
            return true;
        }

        private bool TryParseType(out TypeNode result)
        {
            result = null;

            var startToken = EatToken();
            switch (startToken.Kind)
            {
                case TokenKind.Identifier:
                    result = new NamedType(startToken.Position, startToken.Value);
                    break;

                case TokenKind.At: {
                        if (!TryParseType(out TypeNode poitingTo))
                            return false;

                        result = new PointerType(startToken.Position, poitingTo, PointerType.Kind.Normal);
                        break;
                    }

                case TokenKind.UAt: {
                        if (!TryParseType(out TypeNode poitingTo))
                            return false;

                        result = new PointerType(startToken.Position, poitingTo, PointerType.Kind.Unique);
                        break;
                    }

                case TokenKind.SAt: {
                        if (!TryParseType(out TypeNode poitingTo))
                            return false;

                        result = new PointerType(startToken.Position, poitingTo, PointerType.Kind.Shared);
                        break;
                    }

                case TokenKind.SquareLeft:
                    var dimensions = 1;
                    while (TryEatToken(TokenKind.Comma, out Token comman))
                        dimensions++;

                    if (TryEatToken(TokenKind.SquareRight, out Token expectedSquareRight))
                    {
                        if (!TryParseType(out TypeNode arrayOf))
                            return false;

                        result = new ArrayType(startToken.Position, dimensions, arrayOf);
                    }
                    else
                    {
                        // TODO: better error message
                        _compiler.ReportError(expectedSquareRight.Position, "Error");
                        return false;
                    }
                    break;

                case TokenKind.KeywordFunction:
                case TokenKind.KeywordProcedure:
                    var argumentTypes = new List<TypeNode>();

                    if (!PeekIs(TokenKind.ParenthesesRight))
                    {
                        if (!TryParseType(out TypeNode argumentType))
                            return false;

                        argumentTypes.Add(argumentType);

                        while (!PeekIs(TokenKind.ParenthesesRight))
                        {
                            if (TryEatToken(TokenKind.Comma, out Token expectedComman))
                            {
                                if (!TryParseType(out argumentType))
                                    return false;
                            }
                            else
                            {
                                // TODO: better error message
                                _compiler.ReportError(expectedComman.Position, "Error");
                                return false;
                            }
                        }

                    }

                    if (TryEatToken(TokenKind.Arrow, out Token arrow))
                    {
                        if (!TryParseType(out TypeNode returnType))
                            return false;

                        result = new ProcedureType(startToken.Position, returnType, argumentTypes, startToken.Kind == TokenKind.KeywordFunction);
                    }
                    else
                    {
                        // TODO: better error message
                        _compiler.ReportError(arrow.Position, "Error");
                        return false;
                    }
                    break;

                case TokenKind.ParenthesesLeft:
                    var itemTypes = new List<TypeNode>();
                    
                    if (!PeekIs(TokenKind.ParenthesesRight))
                    {
                        if (!TryParseType(out TypeNode argumentType))
                            return false;

                        itemTypes.Add(argumentType);

                        while (!PeekIs(TokenKind.ParenthesesRight))
                        {
                            if (TryEatToken(TokenKind.Comma, out Token expectedComman))
                            {
                                if (!TryParseType(out argumentType))
                                    return false;
                            }
                            else
                            {
                                // TODO: better error message
                                _compiler.ReportError(expectedComman.Position, "Error");
                                return false;
                            }
                        }

                    }

                    result = new TupleType(startToken.Position, itemTypes);
                    break;

                default:
                    // TODO: better error message
                    _compiler.ReportError(startToken.Position, "Error");
                    return false;
            }

            return true;
        }
        
        private bool TryParseExpression(out Expression result)
        {
            result = null;

            if (!TryParseTerm(out Expression top))
                return false;
            
            for (;;)
            {
                BinaryOperator op = null;
                var peek = PeekToken();

                // This switch only checks if the cast actually maps to any BinaryOperatorKind
                switch ((BinaryOperatorKind)peek.Kind)
                {
                    case BinaryOperatorKind.Dot:
                    case BinaryOperatorKind.Times:
                    case BinaryOperatorKind.Divide:
                    case BinaryOperatorKind.Modulo:
                    case BinaryOperatorKind.Plus:
                    case BinaryOperatorKind.Minus:
                    case BinaryOperatorKind.LessThan:
                    case BinaryOperatorKind.LessThanEqual:
                    case BinaryOperatorKind.GreaterThan:
                    case BinaryOperatorKind.GreaterThanEqual:
                    case BinaryOperatorKind.EqualEqual:
                    case BinaryOperatorKind.ExclamationMarkEqual:
                        op = new BinaryOperator(peek.Position) { Kind = (BinaryOperatorKind)peek.Kind };
                        break;
                }

                if (op == null)
                    break;

                EatToken();

                if (!TryParseTerm(out Expression right))
                    return false;
                
                op.Left = top;
                op.Right = right;
                top = op;

                // The loop that ensures that the operators upholds their priority.
                while (op.Left is BinaryOperator b)
                {
                    if (b.Priority <= op.Priority)
                        break;

                    op.Left = b.Right;
                    b.Right = op;
                }
            }

            result = top;
            return true;
        }

        private bool TryParseTerm(out Expression result)
        {
            throw new NotImplementedException();
        }

        private void InitParser()
        {
            _filesInProject = new HashSet<string>();
            _filesToCompile = new Queue<string>();
        }

        private void InitParser(TextReader stream)
        {
            InitParser();
            _currentScanner = new Scanner(stream);
        }

        private void InitParser(string fileName)
        {
            InitParser();
            _currentScanner = new Scanner(fileName);
            _filesInProject.Add(fileName);
        }

        private Token PeekToken(int offset = 0) => _currentScanner.Peek(offset);
        private Token EatToken() => _currentScanner.Eat();
        private bool PeekIs(TokenKind expectedKind, int offset = 0) => PeekToken(offset).Kind == expectedKind;

        /// <summary>
        /// Try to eat a token of a certain kind.
        /// Result will always be the peek token regardless of wether the token was eaten or not.
        /// </summary>
        /// <param name="expectedKind"></param>
        /// <param name="result">The peek token</param>
        /// <returns></returns>
        private bool TryEatToken(TokenKind expectedKind, out Token result)
        {
            result = PeekToken();
            if (result.Kind != expectedKind)
                return false;

            EatToken();
            return true;
        }
    }
}
