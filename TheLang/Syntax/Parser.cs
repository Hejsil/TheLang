using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheLang.AST;
using TheLang.AST.Bases;
using TheLang.AST.Expressions;
using TheLang.AST.Expressions.Literals;
using TheLang.AST.Expressions.Operators.Binary;
using TheLang.AST.Expressions.Operators.Unary;
using TheLang.AST.Expressions.Types;
using TheLang.AST.Statments;

namespace TheLang.Syntax
{
    public class Parser : Scanner
    {
        public static readonly Dictionary<Type, OpInfo> OperatorInfo =
            new Dictionary<Type, OpInfo>
        {
            { typeof(ASTParentheses), new OpInfo(int.MinValue, Associativity.LeftToRight) },

            { typeof(ASTDot), new OpInfo(0, Associativity.LeftToRight) },

            { typeof(ASTCall), new OpInfo(1, Associativity.LeftToRight) },
            { typeof(ASTIndexing), new OpInfo(1, Associativity.LeftToRight) },

            { typeof(ASTReference), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(ASTDereference), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(ASTNegative), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(ASTPositive), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(ASTNot), new OpInfo(2, Associativity.RightToLeft) },

            { typeof(ASTAs), new OpInfo(3, Associativity.LeftToRight) },

            { typeof(ASTTimes), new OpInfo(4, Associativity.LeftToRight) },
            { typeof(ASTDivide), new OpInfo(4, Associativity.LeftToRight) },
            { typeof(ASTModulo), new OpInfo(4, Associativity.LeftToRight) },

            { typeof(ASTAdd), new OpInfo(5, Associativity.LeftToRight) },
            { typeof(ASTSub), new OpInfo(5, Associativity.LeftToRight) },

            { typeof(ASTLessThan), new OpInfo(6, Associativity.LeftToRight) },
            { typeof(ASTLessThanEqual), new OpInfo(6, Associativity.LeftToRight) },
            { typeof(ASTGreaterThan), new OpInfo(6, Associativity.LeftToRight) },
            { typeof(ASTGreaterThanEqual), new OpInfo(6, Associativity.LeftToRight) },

            { typeof(ASTEqual), new OpInfo(7, Associativity.LeftToRight) },
            { typeof(ASTNotEqual), new OpInfo(7, Associativity.LeftToRight) },

            { typeof(ASTAnd), new OpInfo(8, Associativity.LeftToRight) },
            { typeof(ASTOr), new OpInfo(8, Associativity.LeftToRight) },
        };

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

        /// <summary>
        /// 
        /// 
        /// File syntax:
        ///   File -> ASTDeclaration*
        ///   
        /// </summary>
        /// <returns></returns>
        public ASTFileNode ParseFile()
        {
            var declarations = new List<ASTNode>();
            var start = PeekToken();

            while (!EatToken(TokenKind.EndOfFile))
            {
                if (PeekTokenIs(TokenKind.Identifier))
                {
                    var declaration = ParseDeclaration();
                    if (declaration == null)
                        return null;

                    declarations.Add(declaration);
                }
                else
                {
                    var peek = PeekToken();
                    _compiler.ReportError(peek.Position,
                        $"Expected {TokenKind.Identifier}, but got {peek.Kind}.",
                        "Only declarations can exist in the global scope of a program.");
                    return null;
                }
            }

            return new ASTFileNode(start.Position) { Declarations = declarations };
        }

        /// <summary>
        /// 
        /// 
        /// ASTDeclaration syntax:
        ///   ASTDeclaration -> var Identifier ":" Type
        ///                | (const | var) Identifier ":" Type? "=" Expression
        /// 
        /// </summary>
        /// <returns></returns>
        private ASTDeclaration ParseDeclaration()
        {
            if (!EatToken(TokenKind.KeywordVar) && !EatToken(TokenKind.KeywordConst))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Was trying to parse a ASTDeclaration and expected an {TokenKind.KeywordConst} or {TokenKind.KeywordVar}, but got {peek.Kind}.");
                return null;
            }

            var isConst = EatenToken.Kind == TokenKind.KeywordConst;

            if (!EatToken(TokenKind.Identifier))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Was trying to parse a ASTDeclaration and expected an {TokenKind.Identifier}, but got {peek.Kind}.");
                return null;
            }

            var identifier = EatenToken;
            if (!EatToken(TokenKind.Colon))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Was trying to parse a ASTDeclaration and expected an {TokenKind.Colon}, but got {peek.Kind}.");
                return null;
            }

            ASTNode type;
            if (PeekTokenIs(TokenKind.Equal))
            {
                type = new ASTInfer();
            }
            else
            {
                type = ParseType();
                if (type == null)
                    return null;
            }

            if (EatToken(TokenKind.Equal))
            {
                var colonOrEqual = EatenToken;

                var expression = ParseExpression();
                if (expression == null)
                    return null;

                return new ASTVariable(identifier.Position)
                {
                    IsConstant = isConst,
                    DeclaredType = type,
                    Name = identifier.Value,
                    Value = expression
                };
            }

            return new ASTDeclaration(EatenToken.Position)
            {
                DeclaredType = type,
                Name = identifier.Value
            };

        }

        /// <summary>
        /// 
        /// Expression syntax:
        ///   Expression -> Unary BinaryOperator Unary
        ///
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseExpression()
        {
            var top = ParseUnary();
            if (top == null)
                return null;

            // TODO: This is close, but no quite correct
            while (EatToken(IsBinaryOperator))
            {
                var op = MakeBinaryOperator(EatenToken);

                var right = ParseUnary();
                if (right == null)
                    return null;

                OpInfo opInfo;
                if (!OperatorInfo.TryGetValue(op.GetType(), out opInfo))
                {
                    // TODO: Better error
                    _compiler.ReportError(op.Position, $"Internal parser Error");
                    return null;
                }

                ASTNode prev = null;
                var current = top;
                OpInfo currentInfo;

                // The loop that ensures that the operators upholds their priority.
                while (OperatorInfo.TryGetValue(current.GetType(), out currentInfo) &&
                       opInfo.Priority <= currentInfo.Priority)
                {
                    var unary = current as ASTUnaryNode;
                    var binary = current as ASTBinaryNode;

                    if (unary != null)
                    {
                        if (opInfo.Priority == currentInfo.Priority)
                            break;

                        prev = current;
                        current = unary.Child;
                    }
                    else if ((opInfo.Priority != currentInfo.Priority ||
                             opInfo.Associativity == Associativity.RightToLeft) &&
                             binary != null)
                    {
                        prev = current;
                        current = binary.Right;
                    }
                    else
                    {
                        break;
                    }
                }

                if (op is ASTDot && !(right is ASTSymbol))
                {
                    // TODO: Better error
                    _compiler.ReportError(right.Position,
                        "The right side of the dot operator can only by a symbol.");
                    return null;
                }

                op.Right = right;
                op.Left = current;

                {
                    var unary = prev as ASTUnaryNode;
                    var binary = prev as ASTBinaryNode;

                    if (unary != null)
                        unary.Child = op;
                    else if (binary != null)
                        binary.Right = op;
                    else
                        prev = op;
                }

                top = prev;
            }

            return top;
        }

        /// <summary>
        /// 
        /// Unary syntax:
        ///   Unary -> UnaryOperator* Term ( ASTStructInitializer | ASTCall | ASTIndexing )*
        ///   ASTStructInitializer -> "{" ( Expression ( "," Expression )* ","? )? "}"
        ///   ASTCall -> "(" ( Expression ( "," Expression )* )? ")"
        ///   ASTIndexing -> "[" Expression "]"
        ///   
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseUnary()
        {
            ASTUnaryNode astUnary = null;
            ASTUnaryNode astUnaryChild = null;

            do
            {
                if (astUnary != null)
                    astUnary.Child = astUnaryChild;

                astUnary = astUnaryChild;
                astUnaryChild = null;

                if (EatToken(IsUnaryOperator))
                    astUnaryChild = MakeUnaryOperator(EatenToken);

            } while (astUnaryChild != null);


            var leaf = ParseTerm();
            if (leaf == null)
                return null;


            for (;;)
            {
                if (EatToken(TokenKind.CurlyLeft))
                {
                    var expressions = new List<ASTNode>();

                    if (!EatToken(TokenKind.CurlyRight))
                    {
                        do
                        {
                            var right = ParseExpression();
                            if (right == null)
                                return null;

                            expressions.Add(right);
                        }
                        while (!EatToken(TokenKind.Comma));

                        if (!EatToken(TokenKind.CurlyRight))
                        {
                            // TODO: Error
                            var peek = PeekToken();
                            _compiler.ReportError(peek.Position,
                                $"");
                            return null;
                        }
                    }

                    leaf = new ASTStructInitializer(leaf.Position) { Child = leaf, Values = expressions };
                    continue;
                }

                if (EatToken(TokenKind.ParenthesesLeft))
                {
                    var arguments = new List<ASTNode>();
                    while (!EatToken(TokenKind.ParenthesesRight))
                    {
                        if (arguments.Count != 0 && !EatToken(TokenKind.Comma))
                        {
                            var peek = PeekToken();
                            _compiler.ReportError(peek.Position,
                                $"Found an unexpected token when parsing a procedure call's arguments. Expected {TokenKind.Comma}, but got {peek.Kind}.");
                            return null;
                        }

                        var argument = ParseExpression();
                        if (argument == null)
                            return null;

                        arguments.Add(argument);
                    }

                    leaf = new ASTCall(leaf.Position) { Child = leaf, Arguments = arguments };
                    continue;
                }

                if (EatToken(TokenKind.SquareLeft))
                {
                    var argument = ParseExpression();
                    if (argument == null)
                        return null;

                    if (!EatToken(TokenKind.SquareRight))
                    {
                        var peek = PeekToken();
                        _compiler.ReportError(peek.Position, $"Did not find the end of the indexing.");
                        return null;
                    }

                    leaf = new ASTIndexing(leaf.Position) { Child = leaf, Argument = argument };
                    continue;
                }

                break;
            }

            if (astUnary != null)
            {
                astUnary.Child = leaf;
                return astUnary;
            }

            return leaf;
        }

        /// <summary>
        /// 
        /// Term syntax:
        ///   Term -> Identifier
        ///         | FloatNumber
        ///         | DecimalNumber
        ///         | String
        ///         | "(" Expression ")"
        ///         | ProcedureLiteral
        ///   
        ///   ProcedureLiteral -> ( "func" | "proc" ) "(" ( ASTDeclaration ( "," ASTDeclaration )* )? ")" Expression "=>" ASTCodeBlock
        ///                     | "(" ( Identifier ( "," Identifier )* )? ")" "=>" ASTCodeBlock
        /// 
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseTerm()
        {
            if (EatToken(TokenKind.Identifier))
                return new ASTSymbol(EatenToken.Position, EatenToken.Value);

            if (EatToken(TokenKind.FloatNumber))
                return new ASTFloatLiteral(EatenToken.Position, double.Parse(EatenToken.Value));

            if (EatToken(TokenKind.DecimalNumber))
                return new ASTIntegerLiteral(EatenToken.Position, int.Parse(EatenToken.Value));
            
            if (EatToken(TokenKind.String))
                return new ASTStringLiteral(EatenToken.Position, EatenToken.Value);

            if (EatToken(TokenKind.ParenthesesLeft))
            {
                var start = EatenToken;
                var expression = ParseExpression();
                if (expression == null)
                    return null;

                if (EatToken(TokenKind.ParenthesesRight))
                    return new ASTParentheses(start.Position) { Child = expression };

                _compiler.ReportError(EatenToken.Position,
                    $"Could not find a pair for the left parentheses at {start.Position.Line}:{start.Position.Column}. Expected {TokenKind.ParenthesesRight}, but got {EatenToken.Kind}.");
                return null;
            }
            
            if (EatToken(t => t == TokenKind.KeywordFunction || t == TokenKind.KeywordProcedure))
            {
                var start = EatenToken;
                if (!EatToken(TokenKind.ParenthesesLeft))
                {
                    var peek = PeekToken();
                    _compiler.ReportError(peek.Position,
                        $"Could not find the start parentheses at the start of a procedure's argument. Expected {TokenKind.ParenthesesLeft}, but got {peek.Kind}.");
                    return null;
                }
                
                var arguments = new List<ASTDeclaration>();
                while (!EatToken(TokenKind.ParenthesesRight))
                {
                    if (arguments.Count != 0 && !EatToken(TokenKind.Comma))
                    {
                        var peek = PeekToken();
                        _compiler.ReportError(peek.Position,
                            $"Found an unexpected token when parsing a procedure's arguments. Expected {TokenKind.Comma}, but got {peek.Kind}.");
                        return null;
                    }

                    var declaration = ParseDeclaration();
                    if (declaration == null)
                        return null;

                    arguments.Add(declaration);
                }

                var returnType = ParseExpression();
                if (returnType == null)
                    return null;
                
                var block = ParseCodeBlock();
                if (block == null)
                    return null;

                return new ASTProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                {
                    Block = block,
                    Return = returnType,
                    Arguments = arguments
                };
            }

            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"While parsing a term, the parser found an unexpected token: {peek.Kind}");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ASTCodeBlock ParseCodeBlock()
        {
            if (!EatToken(TokenKind.CurlyLeft))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Did not find the start of a code block. Expected {TokenKind.CurlyLeft}, but got {peek.Kind}");
                return null;
            }

            var start = EatenToken;
            var statements = new List<ASTNode>();

            while (!EatToken(TokenKind.CurlyRight))
            {
                if (PeekTokenIs(TokenKind.Identifier) && PeekTokenIs(TokenKind.Colon, 1))
                {
                    var declaration = ParseDeclaration();
                    if (declaration == null)
                        return null;

                    statements.Add(declaration);
                }
                else
                {
                    var isReturn = EatToken(TokenKind.KeywordReturn);
                    var returnToken = EatenToken;

                    var expression = ParseExpression();
                    if (expression == null)
                        return null;

                    if (isReturn)
                        expression = new Return(returnToken.Position) { Child = expression};

                    statements.Add(expression);
                }
            }

            return new ASTCodeBlock(start.Position) { Statements = statements };
        }

        private static ASTBinaryNode MakeBinaryOperator(Token token)
        {
            var kind = token.Kind;
            var pos = token.Position;
            switch (kind)
            {
                case TokenKind.Plus:
                    return new ASTAdd(pos);
                case TokenKind.Minus:
                    return new ASTSub(pos);
                case TokenKind.Times:
                    return new ASTTimes(pos);
                case TokenKind.Divide:
                    return new ASTDivide(pos);
                case TokenKind.EqualEqual:
                    return new ASTEqual(pos);
                case TokenKind.Modulo:
                    return new ASTModulo(pos);
                case TokenKind.LessThan:
                    return new ASTLessThan(pos);
                case TokenKind.LessThanEqual:
                    return new ASTLessThanEqual(pos);
                case TokenKind.GreaterThan:
                    return new ASTGreaterThan(pos);
                case TokenKind.GreaterThanEqual:
                    return new ASTGreaterThanEqual(pos);
                case TokenKind.ExclamationMarkEqual:
                    return new ASTNotEqual(pos);
                case TokenKind.KeywordAnd:
                    return new ASTAnd(pos);
                case TokenKind.KeywordOr:
                    return new ASTOr(pos);
                case TokenKind.KeywordAs:
                    return new ASTAs(pos);
                case TokenKind.Dot:
                    return new ASTDot(pos);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static ASTUnaryNode MakeUnaryOperator(Token token)
        {
            var kind = token.Kind;
            var pos = token.Position;
            switch (kind)
            {
                case TokenKind.ExclamationMark:
                    return new ASTNot(pos);
                case TokenKind.At:
                    return new ASTReference(pos);
                case TokenKind.Tilde:
                    return new ASTDereference(pos);
                case TokenKind.Plus:
                    return new ASTPositive(pos);
                case TokenKind.Minus:
                    return new ASTNegative(pos);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static readonly TokenKind[] TokenKinds = (TokenKind[])Enum.GetValues(typeof(TokenKind));
        private static readonly IEnumerable<TokenKind> BinaryOperators =
            TokenKinds
                .SkipWhile(t => t < TokenKind.Plus)
                .TakeWhile(t => t <= TokenKind.Dot);
        private static readonly IEnumerable<TokenKind> UnaryOperators =
            TokenKinds
                .SkipWhile(t => t < TokenKind.ExclamationMark)
                .TakeWhile(t => t <= TokenKind.Minus);

        private static bool IsBinaryOperator(TokenKind kind) => BinaryOperators.Contains(kind);
        private static bool IsUnaryOperator(TokenKind kind) => UnaryOperators.Contains(kind);
    }
}