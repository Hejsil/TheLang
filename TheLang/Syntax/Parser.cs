using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using PeterO.Numbers;
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
        private static readonly Dictionary<Type, OpInfo> OperatorInfo =
            new Dictionary<Type, OpInfo>
        {
            { typeof(ASTParentheses),       new OpInfo(9, Associativity.LeftToRight) },
                                            
            { typeof(ASTDot),               new OpInfo(8, Associativity.LeftToRight) },
                                            
            { typeof(ASTCall),              new OpInfo(7, Associativity.LeftToRight) },
            { typeof(ASTIndexing),          new OpInfo(7, Associativity.LeftToRight) },
            { typeof(ASTEmptyInitializer),  new OpInfo(7, Associativity.LeftToRight) },
            { typeof(ASTStructInitializer), new OpInfo(7, Associativity.LeftToRight) },
            { typeof(ASTArrayInitializer),  new OpInfo(7, Associativity.LeftToRight) },
                                            
            { typeof(ASTReference),         new OpInfo(6, Associativity.RightToLeft) },
            { typeof(ASTDereference),       new OpInfo(6, Associativity.RightToLeft) },
            { typeof(ASTNegative),          new OpInfo(6, Associativity.RightToLeft) },
            { typeof(ASTPositive),          new OpInfo(6, Associativity.RightToLeft) },
            { typeof(ASTNot),               new OpInfo(6, Associativity.RightToLeft) },
                                            
            { typeof(ASTAs),                new OpInfo(5, Associativity.LeftToRight) },
                                            
            { typeof(ASTTimes),             new OpInfo(4, Associativity.LeftToRight) },
            { typeof(ASTDivide),            new OpInfo(4, Associativity.LeftToRight) },
            { typeof(ASTModulo),            new OpInfo(4, Associativity.LeftToRight) },
                                            
            { typeof(ASTAdd),               new OpInfo(3, Associativity.LeftToRight) },
            { typeof(ASTSub),               new OpInfo(3, Associativity.LeftToRight) },
                                            
            { typeof(ASTLessThan),          new OpInfo(2, Associativity.LeftToRight) },
            { typeof(ASTLessThanEqual),     new OpInfo(2, Associativity.LeftToRight) },
            { typeof(ASTGreaterThan),       new OpInfo(2, Associativity.LeftToRight) },
            { typeof(ASTGreaterThanEqual),  new OpInfo(2, Associativity.LeftToRight) },
                                            
            { typeof(ASTEqual),             new OpInfo(1, Associativity.LeftToRight) },
            { typeof(ASTNotEqual),          new OpInfo(1, Associativity.LeftToRight) },
                                            
            { typeof(ASTAnd),               new OpInfo(0, Associativity.LeftToRight) },
            { typeof(ASTOr),                new OpInfo(0, Associativity.LeftToRight) },
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
            var start = PeekToken();
            var declarations = ParseMany(TokenKind.EndOfFile, ParseDeclaration);
            return declarations != null ? new ASTFileNode(start.Position) { Declarations = declarations } : null;
        }

        /// <summary>
        /// 
        /// 
        /// ASTDeclaration syntax:
        ///   ASTDeclaration -> var Identifier ":" TypeInfo
        ///                | (const | var) Identifier "=" Expression
        ///                | (const | var) Identifier ":" TypeInfo "=" Expression
        /// 
        /// </summary>
        /// <returns></returns>
        private ASTDeclaration ParseDeclaration()
        {
            var start = PeekToken();
            if (!EatToken(TokenKind.KeywordVar) && !EatToken(TokenKind.KeywordConst))
            {
                Error(start.Position,
                    $"Was trying to parse a ASTDeclaration and expected an {TokenKind.KeywordConst} or {TokenKind.KeywordVar}, but got {start.Kind}.");
                return null;
            }
            
            var identifier = PeekToken();
            if (!Expect(TokenKind.Identifier)) return null;
            if (!Expect(TokenKind.Colon)) return null;

            var type = PeekTokenIs(TokenKind.Equal) ? new ASTInfer() : ParseType();
            if (type == null) return null;

            if (EatToken(TokenKind.Equal))
            {
                var colonOrEqual = EatenToken;

                var expression = ParseExpression();
                if (expression == null) return null;

                return new ASTVariable(identifier.Position)
                {
                    IsConstant = start.Kind == TokenKind.KeywordConst,
                    DeclaredType = type,
                    Name = identifier.Value,
                    Value = expression
                };
            }

            if (start.Kind == TokenKind.KeywordConst)
            {
                Error(start.Position, $"A declarations cannot be marked const, without being initialized to a value");
                return null;
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
            if (top == null) return null;

            while (EatToken(IsBinaryOperator))
            {
                var op = MakeBinaryOperator(EatenToken);

                var right = ParseUnary();
                if (right == null)  return null;

                op.Right = right;
                op.Left = top;
                top = op;
            }

            return top;
        }

        /// <summary>
        /// 
        /// Unary syntax:
        ///   Unary -> UnaryOperator* Term ( ASTStructInitializer | ASTEmptyInitializer | ASTEmptyInitializer | ASTCall | ASTIndexing )*
        ///   ASTStructInitializer -> "{" ( Identifier "=" Expression )* "}"
        ///   ASTArrayInitializer  -> "{" Expression+ "}"
        ///   ASTEmptyInitializer  -> "{" "}"
        ///   ASTCall              -> "(" ( Expression ( "," Expression )* )? ")"
        ///   ASTIndexing          -> "[" ( Expression ( "," Expression )* )? "]"
        ///   
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseUnary()
        {
            (ASTUnaryNode top, ASTUnaryNode leaf) GetPrefix()
            {
                if (!EatToken(IsUnaryOperator)) return (null, null);

                var unaryTop = MakeUnaryOperator(EatenToken);
                var (child, unaryLeaf) = GetPrefix();
                unaryTop.Child = child;

                if (unaryLeaf == null) unaryLeaf = unaryTop;
                return (unaryTop, unaryLeaf);
            }

            (ASTNode top, ASTUnaryNode leaf) = GetPrefix();
            var term = ParseTerm();
            if (term == null) return null;

            if (top == null)
                top = term;
            else
                leaf.Child = term;

            while (EatToken(t => t == TokenKind.CurlyLeft ||
                                 t == TokenKind.ParenthesesLeft || 
                                 t == TokenKind.SquareLeft))
            {
                if (EatenToken.Kind == TokenKind.CurlyLeft)
                {

                    // ASTStructInitializer -> "{" ( Identifier "=" Expression )+ "}"
                    if (PeekTokenIs(TokenKind.Identifier) && PeekTokenIs(TokenKind.Equal, 1))
                    {
                        ASTAssign ParseInitialized()
                        {
                            var ident = PeekToken();
                            if (!Expect(TokenKind.Identifier)) return null;
                            if (!Expect(TokenKind.Equal)) return null;

                            var right = ParseExpression();
                            if (right == null) return null;

                            return new ASTAssign(null)
                            {
                                Left = new ASTSymbol(ident.Position, ident.Value),
                                Right = right
                            };
                        }

                        var equals = ParseMany(TokenKind.CurlyRight, ParseInitialized);
                        if (equals == null) return null;

                        top = new ASTStructInitializer(top.Position) {Child = top, Values = equals};
                    }
                    //   ASTArrayInitializer  -> "{" Expression+ "}"
                    //   ASTEmptyInitializer  -> "{" "}"
                    else
                    {
                        var values = ParseMany(TokenKind.CurlyRight, ParseExpression);
                        if (values == null) return null;

                        if (values.Any())
                            top = new ASTArrayInitializer(top.Position) {Child = top, Values = values};
                        else
                            top = new ASTEmptyInitializer(top.Position) {Child = top};
                    }

                }
                //   ASTIndexing -> "[" ( Expression ( "," Expression )* )? "]"
                //   ASTCall -> "(" ( Expression ( "," Expression )* )? ")"
                else
                {
                    var last = EatenToken.Kind == TokenKind.ParenthesesLeft
                        ? TokenKind.ParenthesesRight
                        : TokenKind.SquareRight;

                    var arguments = ParseMany(last, TokenKind.Comma, ParseExpression);
                    if (arguments == null) return null;

                    top = EatenToken.Kind == TokenKind.ParenthesesRight
                        ? (ASTNode) new ASTCall(top.Position)     { Child = top, Arguments = arguments }
                        : (ASTNode) new ASTIndexing(top.Position) { Child = top, Arguments = arguments };

                }
            }

            return top;
        }

        /// <summary>
        /// 
        /// Term syntax:
        ///   Term -> Identifier
        ///         | FloatNumber
        ///         | DecimalNumber
        ///         | String
        ///         | "(" Expression ")"
        ///         | Lambda
        //          | CompilerIdentifier "(" ( Expression ( "," Expression )* )? ")"
        ///   
        ///   Lambda -> KeywordProc "(" (Argument ( "," Argument )* )? ")" ( -> TypeInfo )? 
        ///   Argument -> Identifier ":" TypeInfo ( "=" Expression )?
        /// 
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseTerm()
        {
            // Identifier
            if (EatToken(TokenKind.Identifier))
                return new ASTSymbol(EatenToken.Position, EatenToken.Value);

            // FloatNumber
            if (EatToken(TokenKind.FloatNumber))
                return new ASTFloatLiteral(EatenToken.Position, EDecimal.FromString(EatenToken.Value));

            // DecimalNumber
            if (EatToken(TokenKind.DecimalNumber))
                return new ASTIntegerLiteral(EatenToken.Position, EInteger.FromString(EatenToken.Value));

            // String
            if (EatToken(TokenKind.String))
                return new ASTStringLiteral(EatenToken.Position, EatenToken.Value);

            // "(" Expression ")"
            if (EatToken(TokenKind.ParenthesesLeft))
            {
                var start = EatenToken;
                var expression = ParseExpression();
                if (expression == null) return null;

                return Expect(TokenKind.ParenthesesRight) ? new ASTParentheses(start.Position) { Child = expression } : null;
            }
            
            //   Lambda -> KeywordProc "(" (Argument ( "," Argument )* )? ")" ( -> TypeInfo )?
            if (EatToken(t => t == TokenKind.KeywordFunction || t == TokenKind.KeywordProcedure))
            {
                var start = EatenToken;
                if (!Expect(TokenKind.ParenthesesLeft)) return null;

                var arguments = ParseMany(TokenKind.ParenthesesRight, TokenKind.Comma, () =>
                {
                    var ident = PeekToken();
                    if (!Expect(TokenKind.Identifier)) return null;
                    if (!Expect(TokenKind.Colon)) return null;

                    var type = ParseType();
                    if (type == null) return null;

                    ASTNode defaultValue = null;
                    if (EatToken(TokenKind.Equal))
                    {
                        defaultValue = ParseExpression();
                        if (defaultValue == null) return null;
                    }

                    return new ASTLambda.Argument(ident.Position)
                    {
                        Name = ident.Value,
                        Type = type,
                        DefaultValue = defaultValue
                    };
                });

                if (arguments == null) return null;

                ASTNode returnType = null;
                if (EatToken(TokenKind.Arrow))
                {
                    returnType = ParseType();
                    if (returnType == null) return null;
                }
                
                var block = ParseCodeBlock();
                if (block == null) return null;

                return new ASTLambda(start.Position)
                {
                    Block = block,
                    Return = returnType,
                    Arguments = arguments
                };
            }

            if (PeekTokenIs(TokenKind.CompilerIdentifier))
                return ParseCompilerCall();

            Error(peek => $"While parsing a term, the parser found an unexpected token: {peek.Kind}");
            return null;
        }

        private ASTCompilerCall ParseCompilerCall()
        {
            var ident = PeekToken();
            if (!Expect(TokenKind.CompilerIdentifier) || !Expect(TokenKind.ParenthesesLeft))
                return null;

            var arguments = ParseMany(TokenKind.ParenthesesRight, TokenKind.Comma, ParseExpression);
            if (arguments == null) return null;
            if (_compiler.Functions.TryGetValue(ident.Value, out var builtIn))
                return new ASTCompilerCall(ident.Position, builtIn) { Arguments = arguments };

            Error(ident.Position, $"#{ident.Value} is not a compile time procedure");
            return null;
        }

        /// <summary>
        /// CodeBlock syntax:
        ///   CodeBlock -> "{" Statement* "}"
        /// </summary>
        /// <returns></returns>
        private ASTCodeBlock ParseCodeBlock()
        {
            var start = PeekToken();
            if (!Expect(TokenKind.CurlyLeft)) return null;

            var statements = ParseMany(TokenKind.CurlyRight, ParseStatement);
            if (statements == null) return null;

            return new ASTCodeBlock(start.Position) { Statements = statements };
        }

        /// <summary>
        /// Statement syntax:
        ///   Statement -> Declaration
        ///              | "return"? Expression
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseStatement()
        {
            if (PeekTokenIs(TokenKind.Identifier) && PeekTokenIs(TokenKind.Colon, 1))
            {
                return ParseDeclaration();
            }
            else
            {
                var isReturn = EatToken(TokenKind.KeywordReturn);
                var returnToken = EatenToken;
                var statement = ParseExpression();
                if (statement == null) return null;
                
                return isReturn ? new ASTReturn(returnToken.Position) { Child = statement } : statement;
            }
        }

        /// <summary>
        /// TypeInfo syntax:
        ///   TypeInfo -> ("@" | "[" "]" ) TypeInfo
        ///         | "proc" "(" ( TypeInfo ( "," TypeInfo )*)? ")" "->" TypeInfo
        ///         | Identifier
        /// </summary>
        /// <returns></returns>
        private ASTNode ParseType()
        {
            var start = PeekToken();

            if (EatToken(TokenKind.Identifier))
                return new ASTSymbol(start.Position, start.Value);

            if (EatToken(TokenKind.At))
            {
                var subType = ParseType();
                if (subType == null) return null;

                return new ASTPointerType(start.Position) { Child = subType };
            }

            if (EatToken(TokenKind.SquareLeft))
            {
                if (!Expect(TokenKind.SquareRight)) return null;

                var subType = ParseType();
                if (subType == null) return null;

                return new ASTArrayType(start.Position) { Child = subType };

            }

            if (EatToken(TokenKind.KeywordProcedure))
            {
                if (!Expect(TokenKind.ParenthesesLeft)) return null;

                var types = ParseMany(TokenKind.ParenthesesRight, TokenKind.Comma, ParseType);
                if (!Expect(TokenKind.Arrow)) return null;

                var returnType = ParseType();
                if (returnType == null) return null;

                return new ASTProcedureType(start.Position) { Arguments = types, Return = returnType };
            }

            Error(start.Position, "");
            return null;
        }

        private IEnumerable<T> ParseMany<T>(TokenKind end, Func<int, T> parseElement) => ParseMany(token => token == end, parseElement);
        private IEnumerable<T> ParseMany<T>(Predicate<TokenKind> end, Func<int, T> parseElement)
        {
            var result = new List<T>();
            while (!EatToken(end))
            {
                var element = parseElement(result.Count);
                if (Equals(element, default(T))) return null;

                result.Add(element);
            }

            return result;
        }

        private IEnumerable<T> ParseMany<T>(Predicate<TokenKind> end, Func<T> parseElement) => ParseMany(end, count => parseElement());
        private IEnumerable<T> ParseMany<T>(TokenKind end, Func<T> parseElement) => ParseMany(end, count => parseElement());

        private IEnumerable<T> ParseMany<T>(TokenKind end, TokenKind splitter, Func<T> parseElement) => 
            ParseMany(token => token == end, token => token == splitter, parseElement);

        private IEnumerable<T> ParseMany<T>(Predicate<TokenKind> end, TokenKind splitter, Func<T> parseElement) =>
            ParseMany(end, token => token == splitter, parseElement);

        private IEnumerable<T> ParseMany<T>(TokenKind end, Predicate<TokenKind> splitter, Func<T> parseElement) =>
            ParseMany(token => token == end, splitter, parseElement);

        private IEnumerable<T> ParseMany<T>(Predicate<TokenKind> end, Predicate<TokenKind> splitter, Func<T> parseElement)
        {
            return ParseMany(end, count =>
            {
                if (count != 0 && !Expect(splitter)) return default(T);
                return parseElement();
            });
        }

        private void Error(Func<Token, string> errorBuilder)
        {
            var peek = PeekToken();
            _compiler.ReportError(peek.Position, nameof(Parser), errorBuilder(peek));
        }

        private void Error(Position position, string error)
        {
            _compiler.ReportError(position, nameof(Parser), error);
        }

        private bool Expect(TokenKind expected)
        {
            if (EatToken(expected)) return true;

            Error(peek => $"Expected {expected}, but got {peek.Kind}");
            return false;
        }

        private bool Expect(Predicate<TokenKind> expected)
        {
            if (EatToken(expected)) return true;

            Error(peek => $"Expected {expected}, but got {peek.Kind}");
            return false;
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

        private static readonly IReadOnlyList<TokenKind> TokenKinds = (TokenKind[])Enum.GetValues(typeof(TokenKind));
        private static readonly HashSet<TokenKind> BinaryOperators = 
            new HashSet<TokenKind>(TokenKinds
                .SkipWhile(t => t < TokenKind.Plus)
                .TakeWhile(t => t <= TokenKind.Dot));
        private static readonly HashSet<TokenKind> UnaryOperators =
            new HashSet<TokenKind>(TokenKinds
                .SkipWhile(t => t < TokenKind.ExclamationMark)
                .TakeWhile(t => t <= TokenKind.Minus));

        private static bool IsBinaryOperator(TokenKind kind) => BinaryOperators.Contains(kind);
        private static bool IsUnaryOperator(TokenKind kind) => UnaryOperators.Contains(kind);
    }
}