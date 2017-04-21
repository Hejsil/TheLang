using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators;
using TheLang.AST.Statments;

namespace TheLang.Syntax
{
    public class Parser : Scanner
    {
        private readonly Compiler _compiler;

        public Parser(string fileName, Compiler compiler)
            : base(fileName)
        {
            _compiler = compiler;
        }

        public Parser(TextReader reader, Compiler compiler)
            : base(reader)
        {
            _compiler = compiler;
        }

        public bool TryParseFile(out FileNode result)
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
                                    DeclaredType = declaration.DeclaredType,
                                    Name = declaration.Name,
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
                    _compiler.ReportError(peek.Position, 
                        $"Expected {TokenKind.Identifier}, but got {peek.Kind}.", 
                        "Only declarations can exist in the global scope of a program.");
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
                _compiler.ReportError(identifier.Position,
                    $"Was trying to parse a Declaration and expected an {TokenKind.Identifier}, but got {identifier.Kind}.");
                return false;
            }

            if (!TryEatToken(TokenKind.Colon, out var colon))
            {
                _compiler.ReportError(colon.Position,
                    $"Was trying to parse a Declaration and expected an {TokenKind.Colon}, but got {colon.Kind}.");
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
                Name = identifier.Value
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
                    if (b.Priority < op.Priority)
                        break;
                    if (b.Priority == op.Priority && op.Associativity != AssociativityKind.RightToLeft)
                        break;

                    op.Left = b.Right;
                    b.Right = op;
                }

                if (op.Kind == BinaryOperatorKind.Dot && !(op.Right is Symbol))
                {
                    _compiler.ReportError(op.Right.Position, 
                        "Only an identifier is allowed on the right side of the dot operator.");
                    return false;
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

                if (TryEatToken(TokenKind.SquareLeft, out var first))
                {
                    var dimensions = 1;
                    while (TryEatToken(TokenKind.Comma))
                        dimensions++;

                    if (TryEatToken(TokenKind.SquareRight, out var last))
                    {
                        prefix = null;

                        _compiler.ReportError(last.Position, 
                            $"Could not find the end of the array prefix. Expected {TokenKind.SquareRight}, but got {last.Kind}.");
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
                            _compiler.ReportError(equal.Position,
                                $"Can only assign in the curly brackets of an composit type literal. Expected {TokenKind.Equal}, but got {equal.Kind}.");
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
                        _compiler.ReportError(peek.Position,
                            $"Did not find the end of the composit type literal. Expected {TokenKind.CurlyRight}, but got {peek.Kind}.");
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
                            _compiler.ReportError(comma.Position,
                                $"Found an unexpected token when parsing a procedure call's arguments. Expected {TokenKind.Comma}, but got {comma.Kind}.");
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
                if (!TryParseExpression(out result))
                    return false;

                if (TryEatToken(TokenKind.ParenthesesRight, out var parRight))
                    return true;

                _compiler.ReportError(parRight.Position,
                    $"Could not find a pair for the left parentheses at {start.Position.Line}:{start.Position.Column}. Expected {TokenKind.ParenthesesRight}, but got {parRight.Kind}.");
                return false;
            }

            if (start.Kind == TokenKind.KeywordFunction || start.Kind == TokenKind.KeywordProcedure)
            {
                if (!TryEatToken(TokenKind.ParenthesesLeft, out var parLeft))
                {
                    _compiler.ReportError(parLeft.Position,
                        $"Could not find the start parentheses at the start of a procedure's argument. Expected {TokenKind.ParenthesesLeft}, but got {parLeft.Kind}.");
                    return false;
                }
                
                var arguments = new List<Node>();

                while (!TryEatToken(TokenKind.ParenthesesRight))
                {
                    if (arguments.Count != 0 && !TryEatToken(TokenKind.Comma, out var comma))
                    {
                        _compiler.ReportError(comma.Position,
                            $"Found an unexpected token when parsing a procedure's arguments. Expected {TokenKind.Comma}, but got {comma.Kind}.");
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
            
            _compiler.ReportError(start.Position, 
                $"While parsing a term, the parser found an unexpected token: {start.Kind}");
            return false;
        }

        private bool TryParseCodeBlock(out CodeBlock result)
        {
            result = null;

            if (!TryEatToken(TokenKind.CurlyLeft, out var curlyLeft))
            {
                _compiler.ReportError(curlyLeft.Position,
                    $"Did not find the start of a code block. Expected {TokenKind.CurlyLeft}, but got {curlyLeft.Kind}");
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
                                    DeclaredType = declaration.DeclaredType,
                                    Name = declaration.Name,
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