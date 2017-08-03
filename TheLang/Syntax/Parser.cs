using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
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
        ///   ASTDeclaration -> var Identifier ":" Type
        ///                | (const | var) Identifier "=" Expression
        ///                | (const | var) Identifier ":" Type "=" Expression
        /// 
        /// </summary>
        /// <returns></returns>
        private ASTDeclaration ParseDeclaration()
        {
            var start = PeekToken();
            if (!EatToken(TokenKind.KeywordVar) && !EatToken(TokenKind.KeywordConst))
            {
                ErrorHere(start.Position,
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
                ErrorHere(start.Position, $"A declarations cannot be marked const, without being initialized to a value");
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

            var unaries = new List<ASTNode>();
            var binaries = ParseMany(IsBinaryOperator, () =>
            {
                var op = MakeBinaryOperator(EatenToken);

                var right = ParseUnary();
                if (right == null) return null;
                unaries.Add(right);

                return op;
            });

            if (binaries == null) return null;

            // TODO: Do the precedence thing
            foreach (var (binary, unary) in binaries.Zip(unaries, (binary, unary) => (binary, unary)))
            {
                OperatorInfo.TryGetValue(top.GetType(),    out var topInfo);
                OperatorInfo.TryGetValue(binary.GetType(), out var binaryInfo);
                OperatorInfo.TryGetValue(unary.GetType(),  out var unaryInfo);


            }

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
            var prefixOperators = ParseMany(token => !IsUnaryOperator(token), () =>
            {
                // ParseMany, will loop and eat tokens. We can therefor expect that IsUnaryOperator(EatenToken) == true 
                return MakeUnaryOperator(EatenToken);
            });

            // This should never happen, but we check this for future proofness
            if (prefixOperators == null) return null;
            
            var top = ParseTerm();
            if (top == null) return null;

            var postfixOperators = ParseMany<ASTUnaryNode>(
                token => token != TokenKind.CurlyLeft &&
                         token != TokenKind.ParenthesesLeft &&
                         token != TokenKind.SquareLeft,
                () =>
                {
                    if (EatenToken.Kind == TokenKind.CurlyLeft)
                    {
                        // ASTEmptyInitializer -> "{" "}"
                        if (EatToken(TokenKind.CurlyRight))
                        {
                            return new ASTEmptyInitializer(null);
                        }

                        // ASTStructInitializer -> "{" ( Identifier "=" Expression )+ "}"
                        if (PeekTokenIs(TokenKind.Equal, 1))
                        {
                            ASTEqual ParseInitialized()
                            {
                                var ident = PeekToken();
                                if (!Expect(TokenKind.Identifier)) return null;
                                if (!Expect(TokenKind.Equal)) return null;

                                var right = ParseExpression();
                                if (right == null) return null;

                                return new ASTEqual(null)
                                {
                                    Left = new ASTSymbol(ident.Position, ident.Value),
                                    Right = right
                                };
                            }

                            var equals = ParseMany(TokenKind.CurlyRight, ParseInitialized);
                            if (equals == null) return null;

                            return new ASTStructInitializer(null) { Values = equals };
                        }

                        //   ASTArrayInitializer  -> "{" Expression+ "}"
                        var values = ParseMany(TokenKind.CurlyRight, ParseExpression);
                        if (values == null) return null;

                        return new ASTArrayInitializer(null) { Values = values };
                    }

                    //   ASTCall -> "(" ( Expression ( "," Expression )* )? ")"
                    if (EatenToken.Kind == TokenKind.ParenthesesLeft)
                    {
                        var arguments = ParseMany(TokenKind.ParenthesesRight, TokenKind.Comma, ParseExpression);
                        if (arguments == null) return null;

                        return new ASTCall(null) { Arguments = arguments };
                    }

                    //   ASTIndexing -> "[" ( Expression ( "," Expression )* )? "]"
                    if (EatToken(TokenKind.SquareLeft))
                    {
                        var arguments = ParseMany(TokenKind.SquareRight, TokenKind.Comma, ParseExpression);
                        if (arguments == null) return null;

                        return new ASTIndexing(null) { Arguments = arguments };
                    }

                    throw new NotImplementedException();
                });

            if (postfixOperators == null) return null;


            // Reverse prefixOperators because of this:
            // If we iterated in normal order and just set top to be the next in the iteration, then we flip the operators:
            // @+-~ -> ~-+@
            // This is not the case for our postfix operators
            using (var prefixEnum = prefixOperators.Reverse().GetEnumerator())
            using (var postfixEnum = postfixOperators.GetEnumerator())
            {
                postfixEnum.MoveNext();
                prefixEnum.MoveNext();

                while (prefixEnum.Current != null && postfixEnum.Current != null)
                {
                    var preCurrent = prefixEnum.Current;
                    var postCurrent = postfixEnum.Current;
                    OperatorInfo.TryGetValue(preCurrent.GetType(), out var preInfo);
                    OperatorInfo.TryGetValue(postCurrent.GetType(), out var postInfo);

                    if (preInfo.Priority > postInfo.Priority)
                    {
                        preCurrent.Child = top;
                        top = preCurrent;

                        prefixEnum.MoveNext();
                    }
                    else // We pick postfix, if their priorities are equal
                    {
                        postCurrent.Child = top;
                        postCurrent.Position = top.Position; // Postfix operators have the position of their child
                        top = postCurrent;

                        postfixEnum.MoveNext();
                    }
                }

                // TODO: Figure out if we can handle this in the main loop
                while (prefixEnum.Current != null)
                {
                    var preCurrent = prefixEnum.Current;
                    preCurrent.Child = top;
                    top = preCurrent;

                    prefixEnum.MoveNext();
                }

                while (postfixEnum.Current != null)
                {
                    var postCurrent = postfixEnum.Current;
                    postCurrent.Child = top;
                    postCurrent.Position = top.Position; // Postfix operators have the position of their child
                    top = postCurrent;

                    postfixEnum.MoveNext();
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
        ///   
        ///   Lambda -> KeywordProc "(" (Argument ( "," Argument )* )? ")" ( -> Type )? 
        ///   Argument -> Identifier ":" Type ( "=" Expression )?
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
                return new ASTFloatLiteral(EatenToken.Position, double.Parse(EatenToken.Value));

            // DecimalNumber
            if (EatToken(TokenKind.DecimalNumber))
                return new ASTIntegerLiteral(EatenToken.Position, int.Parse(EatenToken.Value));

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
            
            //   Lambda -> KeywordProc "(" (Argument ( "," Argument )* )? ")" ( -> Type )?
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
                        Symbol = new ASTSymbol(ident.Position, ident.Value),
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

            ErrorHere(peek => $"While parsing a term, the parser found an unexpected token: {peek.Kind}");
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
                
                return isReturn ? new Return(returnToken.Position) { Child = statement } : statement;
            }
        }

        private IEnumerable<T> ParseMany<T>(TokenKind end, Func<int, T> parseElement) => ParseMany(token => token == end, parseElement);
        private IEnumerable<T> ParseMany<T>(Predicate<TokenKind> end, Func<int, T> parseElement)
        {
            var result = new List<T>();
            while (!EatToken(end))
            {
                var element = parseElement(result.Count);
                if (element.Equals(default(T))) return null;

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

        private void ErrorHere(Func<Token, string> errorBuilder)
        {
            var peek = PeekToken();
            _compiler.ReportError(peek.Position, errorBuilder(peek));
        }

        private void ErrorHere(Position position, string error)
        {
            _compiler.ReportError(position, error);
        }

        private bool Expect(TokenKind expected) => Expect(token => token == expected);
        private bool Expect(Predicate<TokenKind> expected)
        {
            if (EatToken(expected)) return true;

            ErrorHere(peek => $"Expected {expected}, but got {peek.Kind}");
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