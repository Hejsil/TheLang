using System;
using System.Collections.Generic;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;

namespace TheLang.Syntax
{
    public class Parser
    {
        private readonly Compiler _compiler;
        private readonly Scanner _scanner;

        public Parser(Scanner scanner, Compiler compiler)
        {
            _compiler = compiler;
            _scanner = scanner;
        }

        public bool TryParseProgram(out FileNode result)
        {
            result = null;

            var declarations = new List<Node>();
            var start = PeekToken();

            while (!TryEatToken(TokenKind.EndOfFile, out var peek))
            {
                if (peek.Kind == TokenKind.Identifier)
                {
                    if (!TryParseDeclaration(out var declaration))
                        return false;

                    peek = PeekToken();
                    switch (peek.Kind)
                    {
                        case TokenKind.Equal:
                        case TokenKind.Colon:
                            EatToken();
                            if (!TryParseExpression(out var expression))
                                return false;

                            declarations.Add(
                                new Variable(peek.Position, peek.Kind == TokenKind.Colon)
                                {
                                    Declaration = declaration,
                                    Value = expression
                                });

                            break;
                        default:
                            declarations.Add(declaration);
                            break;
                    }
                }
                else
                {
                    // TODO: better error message
                    _compiler.ReportError(peek.Position, "Error");
                    return false;
                }
            }

            result = new FileNode(start.Position) { Declarations = declarations };
            return true;
        }

        private bool TryParseDeclaration(out Declaration result)
        {
            result = null;

            if (!TryEatToken(TokenKind.Identifier, out var identifier))
            {
                // TODO: better error message
                _compiler.ReportError(identifier.Position, "Error");
                return false;
            }

            if (!TryEatToken(TokenKind.Colon, out var colon))
            {
                // TODO: better error message
                _compiler.ReportError(colon.Position, "Error");
                return false;
            }

            Node type;
            var next = PeekToken();
            if (next.Kind == TokenKind.Colon || next.Kind == TokenKind.Equal)
            {
                type = new NeedsToBeInfered(next.Position);
            }
            else
            {
                if (!TryParseExpression(out type))
                    return false;
            }


            result = new Declaration(identifier.Position)
            {
                DeclaredType = type,
                Name = new Symbol(identifier.Position, identifier.Value)
            };
            return true;
        }

        private bool TryParseExpression(out Node result)
        {
            result = null;

            if (!TryParseUnary(out var top))
                return false;

            var peek = PeekToken();
            while (IsBinaryOperator(peek.Kind))
            {
                var op = new BinaryOperator(peek.Position, (BinaryOperatorKind)peek.Kind);
                EatToken();

                if (!TryParseUnary(out var right))
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

                peek = PeekToken();
            }

            result = top;
            return true;
        }

        private bool TryParseUnary(out Node result)
        {
            #region Local Functions

            bool TryParseUnaryOperatorOrArrayTypePrefix(out UnaryNode prefix)
            {
                var peekToken = PeekToken();

                if (IsUnaryOperator(peekToken.Kind))
                {
                    EatToken();
                    prefix = new UnaryOperator(peekToken.Position, (UnaryOperatorKind)peekToken.Kind);
                    return true;
                }

                if (peekToken.Kind == TokenKind.SquareLeft)
                {
                    if (TryEatToken(TokenKind.SquareLeft, out var first))
                    {
                        // TODO: Error message
                        prefix = null;
                        _compiler.ReportError(first.Position, "");
                        return false;
                    }

                    var dimensions = 1;
                    while (TryEatToken(TokenKind.Comma))
                        dimensions++;

                    if (TryEatToken(TokenKind.SquareRight, out var last))
                    {
                        // TODO: Error message
                        prefix = null;
                        _compiler.ReportError(last.Position, "");
                        return false;
                    }

                    prefix = new ArrayPostfix(first.Position, dimensions);
                    return true;
                }

                prefix = null;
                return true;
            }

            #endregion

            #region Parsing unary prefixes

            result = null;

            if (!TryParseUnaryOperatorOrArrayTypePrefix(out var unary))
                return false;

            if (unary != null)
            {
                result = unary;

                for (;;)
                {
                    if (!TryParseUnaryOperatorOrArrayTypePrefix(out var child))
                        return false;

                    if (child != null)
                    {
                        unary.Child = child;
                        unary = (UnaryNode)unary.Child;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            #endregion

            if (!TryParseTerm(out var leaf))
                return false;

            #region Parsing unary postfixes

            for (;;)
            {
                var peek = PeekToken();

                if (peek.Kind == TokenKind.CurlyLeft)
                {
                    #region Parsing indexing

                    EatToken();
                    var assignments = new List<BinaryOperator>();

                    while (TryEatToken(TokenKind.Identifier, out var identifier))
                    {
                        var left = new Symbol(identifier.Position, identifier.Value);

                        if (!TryEatToken(TokenKind.Equal, out var equal))
                        {
                            // TODO: Error message
                            _compiler.ReportError(equal.Position, "");
                            return false;
                        }

                        if (!TryParseExpression(out var right))
                            return false;

                        assignments.Add(new BinaryOperator(left.Position, BinaryOperatorKind.Assign)
                        {
                            Left = left,
                            Right = right
                        });

                        if (!TryEatToken(TokenKind.Comma))
                            break;
                    }

                    if (!TryEatToken(TokenKind.CurlyRight, out peek))
                    {
                        // TODO: Error message
                        _compiler.ReportError(peek.Position, "");
                        return false;
                    }

                    leaf = new CompositTypeLiteral(leaf.Position) { Child = leaf, Values = assignments };

                    #endregion
                }
                else if (peek.Kind == TokenKind.ParenthesesLeft)
                {
                    #region Parsing call

                    EatToken();
                    var arguments = new List<Node>();

                    while (!TryEatToken(TokenKind.ParenthesesRight))
                    {
                        if (arguments.Count != 0 && !TryEatToken(TokenKind.Comma, out var comma))
                        {
                            // TODO: Error message
                            _compiler.ReportError(comma.Position, "");
                            return false;
                        }

                        if (!TryParseExpression(out var argument))
                            return false;

                        arguments.Add(argument);
                    }

                    leaf = new Call(leaf.Position) { Child = leaf, Arguments = arguments };

                    #endregion
                }
                else
                {
                    break;
                }
            }

            if (unary != null)
                unary.Child = leaf;
            else
                result = leaf;

            return true;

            #endregion
        }

        private bool TryParseTerm(out Node result)
        {
            result = null;
            var start = EatToken();

            if (start.Kind == TokenKind.Identifier)
            {
                result = new Symbol(start.Position, start.Value);
                return true;
            }

            if (start.Kind == TokenKind.FloatNumber)
            {
                result = new FloatLiteral(start.Position, double.Parse(start.Value));
                return true;
            }

            if (start.Kind == TokenKind.DecimalNumber)
            {
                result = new IntegerLiteral(start.Position, int.Parse(start.Value));
                return true;
            }
            
            if (start.Kind == TokenKind.String)
            {
                result = new StringLiteral(start.Position, start.Value);
                return true;
            }

            if (start.Kind == TokenKind.ParenthesesLeft)
            {
                // TODO: Parse par
                _compiler.ReportError(start.Position, "");
                return false;
            }

            if (start.Kind == TokenKind.KeywordFunction || start.Kind == TokenKind.KeywordProcedure)
            {
                if (!TryEatToken(TokenKind.ParenthesesLeft, out var parentheses))
                {
                    // TODO: better error message
                    _compiler.ReportError(parentheses.Position, "Error");
                    return false;
                }
                
                var arguments = new List<Node>();

                while (!TryEatToken(TokenKind.ParenthesesRight))
                {
                    if (arguments.Count != 0 && !TryEatToken(TokenKind.Comma, out var comma))
                    {
                        // TODO: Error message
                        _compiler.ReportError(comma.Position, "");
                        return false;
                    }

                    if (!TryParseExpression(out var argument))
                        return false;

                    arguments.Add(argument);
                }

                Node returnType;
                if (TryEatToken(TokenKind.Arrow, out var arrow))
                {
                    if (!TryParseExpression(out returnType))
                        return false;
                }
                else
                {
                    returnType = new NeedsToBeInfered(arrow.Position);
                }

                var peek = PeekToken();
                switch (peek.Kind)
                {
                    case TokenKind.CurlyLeft:
                        if (!TryParseCodeBlock(out var block))
                            return false;

                        result = new BlockBodyProcedure(start.Position, start.Kind == TokenKind.KeywordFunction)
                        {
                            Block = block,
                            ReturnType = returnType,
                            Arguments = arguments
                        };
                        return true;
                }

                result = new ProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                {
                    Arguments = arguments,
                    ReturnType = returnType
                };
                return true;
            }

            // TODO: Error message
            _compiler.ReportError(start.Position, "");
            return false;
        }

        private bool TryParseCodeBlock(out CodeBlock result)
        {
            result = null;

            if (!TryEatToken(TokenKind.CurlyLeft, out var curlyLeft))
            {
                // TODO: Error message
                _compiler.ReportError(curlyLeft.Position, "");
                return false;
            }

            var statements = new List<Node>();

            while (!TryEatToken(TokenKind.CurlyRight, out var peek))
            {
                if (PeekIs(TokenKind.Identifier) && PeekIs(TokenKind.Colon, 1))
                {
                    if (!TryParseDeclaration(out var declaration))
                        return false;

                    peek = PeekToken();
                    switch (peek.Kind)
                    {
                        case TokenKind.Equal:
                        case TokenKind.Colon:
                            if (!TryParseExpression(out var expression))
                                return false;

                            statements.Add(
                                new Variable(peek.Position, peek.Kind == TokenKind.Colon)
                                {
                                    Declaration = declaration,
                                    Value = expression
                                });

                            break;
                        default:
                            statements.Add(declaration);
                            break;
                    }
                }
                else
                {
                    if (!TryParseExpression(out var expression))
                        return false;

                    statements.Add(expression);
                }
            }

            result = new CodeBlock(curlyLeft.Position) { Statements = statements };
            return true;
        }

        private Token PeekToken(int offset = 0) => _scanner.Peek(offset);
        private Token EatToken() => _scanner.Eat();
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

        /// <summary>
        /// Try to eat a token of a certain kind.
        /// </summary>
        /// <param name="expectedKind"></param>
        /// <returns></returns>
        private bool TryEatToken(TokenKind expectedKind) => TryEatToken(expectedKind, out var _);

        private bool IsBinaryOperator(TokenKind kind)
        {
            var operators = (BinaryOperatorKind[])Enum.GetValues(typeof(BinaryOperatorKind));
            return operators.Contains((BinaryOperatorKind)kind);
        }

        private bool IsUnaryOperator(TokenKind kind)
        {
            var operators = (UnaryOperatorKind[])Enum.GetValues(typeof(UnaryOperatorKind));
            return operators.Contains((UnaryOperatorKind)kind);
        }
    }
}