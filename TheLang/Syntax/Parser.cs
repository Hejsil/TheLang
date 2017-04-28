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
using TheLang.AST.Statments;

namespace TheLang.Syntax
{
    public class Parser : Scanner
    {
        public static readonly Dictionary<Type, OpInfo> OperatorInfo =
            new Dictionary<Type, OpInfo>
        {
            { typeof(Parentheses), new OpInfo(int.MinValue, Associativity.LeftToRight) },

            { typeof(Dot), new OpInfo(0, Associativity.LeftToRight) },

            { typeof(Call), new OpInfo(1, Associativity.LeftToRight) },
            { typeof(Indexing), new OpInfo(1, Associativity.LeftToRight) },

            { typeof(Reference), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(UniqueReference), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(Dereference), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(Negative), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(Positive), new OpInfo(2, Associativity.RightToLeft) },
            { typeof(Not), new OpInfo(2, Associativity.RightToLeft) },

            { typeof(As), new OpInfo(3, Associativity.LeftToRight) },

            { typeof(Times), new OpInfo(4, Associativity.LeftToRight) },
            { typeof(Divide), new OpInfo(4, Associativity.LeftToRight) },
            { typeof(Modulo), new OpInfo(4, Associativity.LeftToRight) },

            { typeof(Add), new OpInfo(5, Associativity.LeftToRight) },
            { typeof(Sub), new OpInfo(5, Associativity.LeftToRight) },

            { typeof(LessThan), new OpInfo(6, Associativity.LeftToRight) },
            { typeof(LessThanEqual), new OpInfo(6, Associativity.LeftToRight) },
            { typeof(GreaterThan), new OpInfo(6, Associativity.LeftToRight) },
            { typeof(GreaterThanEqual), new OpInfo(6, Associativity.LeftToRight) },

            { typeof(Equal), new OpInfo(7, Associativity.LeftToRight) },
            { typeof(NotEqual), new OpInfo(7, Associativity.LeftToRight) },

            { typeof(And), new OpInfo(8, Associativity.LeftToRight) },
            { typeof(Or), new OpInfo(8, Associativity.LeftToRight) },
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
        ///   File -> Declaration*
        ///   
        /// </summary>
        /// <returns></returns>
        public FileNode ParseFile()
        {
            var declarations = new List<Node>();
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

            return new FileNode(start.Position) { Declarations = declarations };
        }

        /// <summary>
        /// 
        /// 
        /// Declaration syntax:
        ///   Declaration -> Identifier ":" Expression
        ///                | Identifier ":" Expression? ( ":" | "=" ) Expression
        /// 
        /// </summary>
        /// <returns></returns>
        private Declaration ParseDeclaration()
        {
            if (!EatToken(TokenKind.Identifier))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Was trying to parse a Declaration and expected an {TokenKind.Identifier}, but got {peek.Kind}.");
                return null;
            }

            var identifier = EatenToken;
            if (!EatToken(TokenKind.Colon))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Was trying to parse a Declaration and expected an {TokenKind.Colon}, but got {peek.Kind}.");
                return null;
            }

            Node type;
            if (PeekTokenIs(t => t == TokenKind.Colon || t == TokenKind.Equal))
            {
                type = new Infer();
            }
            else
            {
                type = ParseExpression();
                if (type == null)
                    return null;
            }

            if (EatToken(t => t.Kind == TokenKind.Colon || t.Kind == TokenKind.Equal))
            {
                var expression = ParseExpression();
                if (expression == null)
                    return null;

                return new Variable(EatenToken.Position, EatenToken.Kind == TokenKind.Colon)
                {
                    DeclaredType = type,
                    Name = identifier.Value,
                    Value = expression
                };
            }

            return new Declaration(EatenToken.Position)
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
        private Node ParseExpression()
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

                Node prev = null;
                var current = top;
                OpInfo currentInfo;

                // The loop that ensures that the operators upholds their priority.
                while (OperatorInfo.TryGetValue(current.GetType(), out currentInfo) &&
                       opInfo.Priority <= currentInfo.Priority)
                {
                    var unary = current as UnaryNode;
                    var binary = current as BinaryNode;

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

                if (op is Dot && !(right is Symbol))
                {
                    // TODO: Better error
                    _compiler.ReportError(right.Position,
                        "The right side of the dot operator can only by a symbol.");
                    return null;
                }

                op.Right = right;
                op.Left = current;

                {
                    var unary = prev as UnaryNode;
                    var binary = prev as BinaryNode;

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
        ///   Unary -> ( UnaryOperator | ArrayTypePrefix )* Term ( TypeLiteral | Call | Indexing )*
        ///   ArrayTypePrefix -> "[" ","* "]"
        ///   TypeLiteral -> "{" ( Expression ( "," Expression )* ","? )? "}"
        ///   Call -> "(" ( Expression ( "," Expression )* )? ")"
        ///   Indexing -> "[" Expression "]"
        ///   
        /// </summary>
        /// <returns></returns>
        private Node ParseUnary()
        {
            UnaryNode unary = null;
            UnaryNode unaryChild = null;

            do
            {
                if (unary != null)
                    unary.Child = unaryChild;

                unary = unaryChild;
                unaryChild = null;

                if (EatToken(IsUnaryOperator))
                {
                    unaryChild = MakeUnaryOperator(EatenToken);
                }
                else if (EatToken(TokenKind.SquareLeft))
                {
                    var first = EatenToken;

                    if (EatToken(TokenKind.SquareRight))
                    {
                        unaryChild = new ArrayPostfix(first.Position) { Size = new Infer() };
                    }
                    else
                    {
                        var expr = ParseExpression();
                        if (expr == null)
                            return null;

                        if (!EatToken(TokenKind.SquareRight))
                        {
                            // TODO: Error
                            var peek = PeekToken();
                            _compiler.ReportError(peek.Position, $"");
                            return null;
                        }

                        unaryChild = new ArrayPostfix(first.Position) { Size = expr };
                    }
                }
            } while (unaryChild != null);


            var leaf = ParseTerm();
            if (leaf == null)
                return null;


            for (;;)
            {
                if (EatToken(TokenKind.CurlyLeft))
                {
                    var assignments = new List<Node>();

                    if (!EatToken(TokenKind.CurlyRight))
                    {
                        do
                        {
                            var right = ParseExpression();
                            if (right == null)
                                return null;

                            assignments.Add(right);
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

                    leaf = new TypeLiteral(leaf.Position) { Child = leaf, Values = assignments };
                    continue;
                }

                if (EatToken(t => t == TokenKind.ParenthesesLeft || t == TokenKind.SquareLeft))
                {
                    var start = EatenToken;

                    var arguments = new List<Node>();
                    while (!EatToken(TokenKind.ParenthesesRight))
                    {
                        if (arguments.Count != 0 && !EatToken(TokenKind.Comma))
                        {
                            var peek = PeekToken();
                            var name = start.Kind == TokenKind.ParenthesesLeft ? "procedure call" : "indexing";
                            _compiler.ReportError(peek.Position,
                                $"Found an unexpected token when parsing a {name}'s arguments. Expected {TokenKind.Comma}, but got {peek.Kind}.");
                            return null;
                        }

                        var argument = ParseExpression();
                        if (argument == null)
                            return null;

                        arguments.Add(argument);
                    }

                    if (start.Kind == TokenKind.ParenthesesLeft)
                        leaf = new Call(leaf.Position) { Child = leaf, Arguments = arguments };
                    else
                        leaf = new Indexing(leaf.Position) { Child = leaf, Arguments = arguments };
                    continue;
                }

                break;
            }

            if (unary != null)
            {
                unary.Child = leaf;
                return unary;
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
        ///         | "struct" "{" Declaration* "}"
        ///         | ProcedureLiteral
        ///         | ProcedureType
        ///   
        ///   ProcedureType -> ( "func" | "proc" ) "(" ( Expression ( "," Expression )* )? ")" Expression
        ///   ProcedureLiteral -> ( "func" | "proc" ) "(" ( Declaration ( "," Declaration )* )? ")" Expression "=>" CodeBlock
        ///                     | "(" ( Identifier ( "," Identifier )* )? ")" "=>" CodeBlock
        /// 
        /// </summary>
        /// <returns></returns>
        private Node ParseTerm()
        {
            if (EatToken(TokenKind.Identifier))
                return new Symbol(EatenToken.Position, EatenToken.Value);

            if (EatToken(TokenKind.FloatNumber))
                return new FloatLiteral(EatenToken.Position, double.Parse(EatenToken.Value));

            if (EatToken(TokenKind.DecimalNumber))
                return new IntegerLiteral(EatenToken.Position, int.Parse(EatenToken.Value));
            
            if (EatToken(TokenKind.String))
                return new StringLiteral(EatenToken.Position, EatenToken.Value);

            if (EatToken(TokenKind.ParenthesesLeft))
            {
                var start = EatenToken;
                var expression = ParseExpression();
                if (expression == null)
                    return null;

                if (EatToken(TokenKind.ParenthesesRight))
                    return expression;

                _compiler.ReportError(EatenToken.Position,
                    $"Could not find a pair for the left parentheses at {start.Position.Line}:{start.Position.Column}. Expected {TokenKind.ParenthesesRight}, but got {EatenToken.Kind}.");
                return null;
            }

            // TODO: This does not parse correctly
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

                // We can't determin from the arguments whether we are a procedure type or literal, if we have no arguments.
                // We therefore have to handle the empty procedure differently
                if (EatToken(TokenKind.ParenthesesRight))
                {
                    var returnType = ParseExpression();
                    if (returnType == null)
                        return null;

                    if (EatToken(TokenKind.Arrow))
                    {
                        var block = ParseCodeBlock();
                        if (block == null)
                            return null;

                        return new TypedProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                        {
                            Block = block,
                            Return = returnType,
                            Arguments = Enumerable.Empty<Declaration>()
                        };
                    }

                    return new ProcedureType(start.Position, start.Kind == TokenKind.KeywordFunction)
                    {
                        Arguments = new []{ returnType }
                    };
                }

                // If first argument looks like a declarations, then we parse the procedure as a literal
                if (PeekTokenIs(TokenKind.Identifier) && PeekTokenIs(TokenKind.Colon, 1))
                {
                    var arguments = new List<Declaration>();

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

                    if (EatToken(TokenKind.Arrow))
                    {
                        var block = ParseCodeBlock();
                        if (block == null)
                            return null;

                        return new TypedProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                        {
                            Block = block,
                            Return = returnType,
                            Arguments = arguments
                        };
                    }

                    {
                        // TODO: Error
                        var peek = PeekToken();
                        _compiler.ReportError(peek.Position, "");
                        return null;
                    }
                }

                // Else we parse it as a type
                {
                    var arguments = new List<Node>();

                    while (!EatToken(TokenKind.ParenthesesRight))
                    {
                        if (arguments.Count != 0 && !EatToken(TokenKind.Comma))
                        {
                            var peek = PeekToken();
                            _compiler.ReportError(peek.Position,
                                $"Found an unexpected token when parsing a procedure's arguments. Expected {TokenKind.Comma}, but got {peek.Kind}.");
                            return null;
                        }

                        var argument = ParseExpression();
                        if (argument == null)
                            return null;

                        arguments.Add(argument);
                    }

                    var returnType = ParseExpression();
                    if (returnType == null)
                        return null;

                    arguments.Add(returnType);
                    return new ProcedureType(start.Position, start.Kind == TokenKind.KeywordFunction)
                    {
                        Arguments = arguments,
                    };
                }
            }

            if (EatToken(TokenKind.KeywordStruct))
            {
                var start = EatenToken;

                if (!EatToken(TokenKind.CurlyLeft))
                {
                    var peek = PeekToken();
                    _compiler.ReportError(peek.Position,
                        $"");
                    return null;
                }

                var fields = new List<Declaration>();
                while (!EatToken(TokenKind.CurlyRight))
                {
                    var decl = ParseDeclaration();
                    if (decl == null)
                        return null;

                    fields.Add(decl);
                }

                return new StructType(start.Position) { Fields = fields };
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
        private CodeBlock ParseCodeBlock()
        {
            if (!EatToken(TokenKind.CurlyLeft))
            {
                var peek = PeekToken();
                _compiler.ReportError(peek.Position,
                    $"Did not find the start of a code block. Expected {TokenKind.CurlyLeft}, but got {peek.Kind}");
                return null;
            }

            var start = EatenToken;
            var statements = new List<Node>();

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

            return new CodeBlock(start.Position) { Statements = statements };
        }

        private static BinaryNode MakeBinaryOperator(Token token)
        {
            var kind = token.Kind;
            var pos = token.Position;
            switch (kind)
            {
                case TokenKind.Plus:
                    return new Add(pos);
                case TokenKind.Minus:
                    return new Sub(pos);
                case TokenKind.Times:
                    return new Times(pos);
                case TokenKind.Divide:
                    return new Divide(pos);
                case TokenKind.EqualEqual:
                    return new Equal(pos);
                case TokenKind.Modulo:
                    return new Modulo(pos);
                case TokenKind.LessThan:
                    return new LessThan(pos);
                case TokenKind.LessThanEqual:
                    return new LessThanEqual(pos);
                case TokenKind.GreaterThan:
                    return new GreaterThan(pos);
                case TokenKind.GreaterThanEqual:
                    return new GreaterThanEqual(pos);
                case TokenKind.ExclamationMarkEqual:
                    return new NotEqual(pos);
                case TokenKind.KeywordAnd:
                    return new And(pos);
                case TokenKind.KeywordOr:
                    return new Or(pos);
                case TokenKind.KeywordAs:
                    return new As(pos);
                case TokenKind.Dot:
                    return new Dot(pos);
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        private static UnaryNode MakeUnaryOperator(Token token)
        {
            var kind = token.Kind;
            var pos = token.Position;
            switch (kind)
            {
                case TokenKind.ExclamationMark:
                    return new Not(pos);
                case TokenKind.At:
                    return new Reference(pos);
                case TokenKind.UAt:
                    return new UniqueReference(pos);
                case TokenKind.Tilde:
                    return new Dereference(pos);
                case TokenKind.Plus:
                    return new Positive(pos);
                case TokenKind.Minus:
                    return new Negative(pos);
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