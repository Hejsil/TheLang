using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

                    declarations.Add(declaration);
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
            var colonOrEqual = PeekToken();
            switch (colonOrEqual.Kind)
            {
                case TokenKind.Colon:
                case TokenKind.Equal:
                    type = new NeedsToBeInfered(colonOrEqual.Position);
                    break;
                default:
                    if (!TryParseExpression(out type))
                        return false;

                    break;
            }


            if (TryEatToken(TokenKind.Colon, out colonOrEqual))
            {
                if (TryEatToken(TokenKind.KeywordStruct))
                {

                    // TODO: Parse struct
                    return true;
                }

                if (!TryParseExpression(out var expression))
                    return false;

                result = new Variable(colonOrEqual.Position, colonOrEqual.Kind == TokenKind.Colon)
                {
                    DeclaredType = type,
                    Name = identifier.Value,
                    Value = expression
                };
                return true;
            }


            if (TryEatToken(TokenKind.Equal, out colonOrEqual))
            {
                if (!TryParseExpression(out var expression))
                    return false;

                result = new Variable(colonOrEqual.Position, colonOrEqual.Kind == TokenKind.Colon)
                {
                    DeclaredType = type,
                    Name = identifier.Value,
                    Value = expression
                };
                return true;
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
                        continue;
                    }

                    break;
                }
            }

            #endregion

            if (!TryParseTerm(out var leaf))
                return false;

            #region Parsing unary postfixes

            for (;;)
            {
                if (TryEatToken(TokenKind.CurlyLeft))
                {
                    #region Parsing indexing
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

                    if (!TryEatToken(TokenKind.CurlyRight, out var curlyRight))
                    {
                        _compiler.ReportError(curlyRight.Position,
                            $"Did not find the end of the composit type literal. Expected {TokenKind.CurlyRight}, but got {curlyRight.Kind}.");
                        return false;
                    }

                    leaf = new CompositTypeLiteral(leaf.Position) { Child = leaf, Values = assignments };
                    continue;

                    #endregion
                }

                if (TryEatToken(TokenKind.ParenthesesLeft))
                {
                    #region Parsing call
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
                    continue;

                    #endregion
                }

                break;
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

            if (TryEatToken(TokenKind.Identifier, out var start))
            {
                result = new Symbol(start.Position, start.Value);
                return true;
            }

            if (TryEatToken(TokenKind.FloatNumber, out start))
            {
                result = new FloatLiteral(start.Position, double.Parse(start.Value));
                return true;
            }

            if (TryEatToken(TokenKind.DecimalNumber, out start))
            {
                result = new IntegerLiteral(start.Position, int.Parse(start.Value));
                return true;
            }
            
            if (TryEatToken(TokenKind.String, out start))
            {
                result = new StringLiteral(start.Position, start.Value);
                return true;
            }

            if (TryEatToken(TokenKind.ParenthesesLeft, out start))
            {
                if (!TryParseExpression(out result))
                    return false;

                if (TryEatToken(TokenKind.ParenthesesRight, out var parRight))
                    return true;

                _compiler.ReportError(parRight.Position,
                    $"Could not find a pair for the left parentheses at {start.Position.Line}:{start.Position.Column}. Expected {TokenKind.ParenthesesRight}, but got {parRight.Kind}.");
                return false;
            }

            if (TryEatToken(TokenKind.KeywordFunction, out start) || TryEatToken(TokenKind.KeywordProcedure, out start))
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
                
                switch (PeekToken().Kind)
                {
                    case TokenKind.CurlyLeft:
                        if (!TryParseCodeBlock(out var block))
                            return false;

                        result = new ProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                        {
                            Block = block,
                            Return = returnType,
                            Arguments = arguments
                        };
                        return true;
                }

                result = new ProcedureTypeNode(start.Position, start.Kind == TokenKind.KeywordFunction)
                {
                    Arguments = arguments,
                    Return = returnType
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

            while (!TryEatToken(TokenKind.CurlyRight))
            {
                if (PeekIs(TokenKind.Identifier) && PeekIs(TokenKind.Colon, 1))
                {
                    if (!TryParseDeclaration(out var declaration))
                        return false;

                    statements.Add(declaration);
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

            Debug.Assert(EatToken().Kind == result.Kind);
            return true;
        }

        /// <summary>
        /// Try to eat a token of a certain kind.
        /// </summary>
        /// <param name="expectedKind"></param>
        /// <returns></returns>
        private bool TryEatToken(TokenKind expectedKind) => TryEatToken(expectedKind, out var _);

        private static readonly HashSet<BinaryOperatorKind> BinaryOperators = 
            new HashSet<BinaryOperatorKind>((BinaryOperatorKind[])Enum.GetValues(typeof(BinaryOperatorKind)));
        private bool IsBinaryOperator(TokenKind kind) => BinaryOperators.Contains((BinaryOperatorKind)kind);
        
        private static readonly HashSet<UnaryOperatorKind> UnaryOperators =
            new HashSet<UnaryOperatorKind>((UnaryOperatorKind[])Enum.GetValues(typeof(UnaryOperatorKind)));
        private bool IsUnaryOperator(TokenKind kind) => UnaryOperators.Contains((UnaryOperatorKind)kind);
    }
}