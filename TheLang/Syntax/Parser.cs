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

        /// <summary>
        /// 
        /// 
        /// File syntax:
        ///   File -> Declaration*
        ///   
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public FileNode TryParseFile()
        {
            var declarations = new List<Node>();
            var start = PeekToken();
            Token peek;

            while (!TryEatToken(TokenKind.EndOfFile, out peek))
            {
                if (peek.Kind == TokenKind.Identifier)
                {
                    var declaration = TryParseDeclaration();
                    if (declaration == null)
                        return null;

                    declarations.Add(declaration);
                }
                else
                {
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
        /// <param name="result"></param>
        /// <returns></returns>
        private Declaration TryParseDeclaration()
        {
            Token identifier;
            Token peek;

            if (!TryEatToken(TokenKind.Identifier, out identifier))
            {
                _compiler.ReportError(identifier.Position,
                    $"Was trying to parse a Declaration and expected an {TokenKind.Identifier}, but got {identifier.Kind}.");
                return null;
            }

            if (!TryEatToken(TokenKind.Colon, out peek))
            {
                _compiler.ReportError(peek.Position,
                    $"Was trying to parse a Declaration and expected an {TokenKind.Colon}, but got {peek.Kind}.");
                return null;
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
                    type = TryParseExpression();
                    if (type == null)
                        return null;

                    break;
            }


            if (TryEatToken(TokenKind.Colon, out colonOrEqual) || TryEatToken(TokenKind.Equal, out colonOrEqual))
            {
                var expression = TryParseExpression();
                if (expression == null)
                    return null;

                return new Variable(colonOrEqual.Position, colonOrEqual.Kind == TokenKind.Colon)
                {
                    DeclaredType = type,
                    Name = peek.Value,
                    Value = expression
                };
            }

            return new Declaration(identifier.Position)
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
        /// <param name="result"></param>
        /// <returns></returns>
        private Node TryParseExpression()
        {
            var top = TryParseUnary();
            if (top == null)
                return null;

            var peek = PeekToken();
            while (IsBinaryOperator(peek.Kind))
            {
                var op = new BinaryOperator(peek.Position, (BinaryOperatorKind)peek.Kind);
                EatToken();

                var right = TryParseUnary();
                if (right == null)
                    return null;

                op.Left = top;
                op.Right = right;
                top = op;

                var b = op.Left as BinaryOperator;
                // The loop that ensures that the operators upholds their priority.
                while (b != null)
                {
                    if (b.Priority < op.Priority)
                        break;
                    if (b.Priority == op.Priority && op.Associativity != AssociativityKind.RightToLeft)
                        break;

                    op.Left = b.Right;
                    b.Right = op;
                    b = op.Left as BinaryOperator;
                }

                if (op.Kind == BinaryOperatorKind.Dot && !(op.Right is Symbol))
                {
                    _compiler.ReportError(op.Right.Position, 
                        "Only an identifier is allowed on the right side of the dot operator.");
                    return null;
                }

                peek = PeekToken();
            }

            return top;
        }

        /// <summary>
        /// 
        /// Unary syntax:
        ///   Unary -> ( UnaryOperator | ArrayTypePrefix )* Term ( CompositTypeLiteral | Call | Indexing )*
        ///   ArrayTypePrefix -> "[" ","* "]"
        ///   CompositTypeLiteral -> "{" ( EqualsExpression ( "," EqualsExpression )* ","? )? "}"
        ///   EqualsExpression -> Indentifier "=" Expression
        ///   Call -> "(" ( Expression ( "," Expression )* )? ")"
        ///   Indexing -> "[" Expression "]"
        ///   
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private Node TryParseUnary()
        {
            var unary = TryParseUnaryOperatorOrArrayTypePrefix();
            var unaryChild = TryParseUnaryOperatorOrArrayTypePrefix();

            while (unaryChild != null)
            {
                unary.Child = unaryChild;
                unary = (UnaryNode)unary.Child;
                unaryChild = TryParseUnaryOperatorOrArrayTypePrefix();
            }

            var leaf = TryParseTerm();

            if (leaf == null)
                return null;
            
            for (;;)
            {
                if (TryEatToken(TokenKind.CurlyLeft))
                {
                    var assignments = new List<BinaryOperator>();

                    Token peek;
                    while (TryEatToken(TokenKind.Identifier, out peek))
                    {
                        var left = new Symbol(peek.Position, peek.Value);

                        if (!TryEatToken(TokenKind.Equal, out peek))
                        {
                            _compiler.ReportError(peek.Position,
                                $"Can only assign in the curly brackets of an composit type literal. Expected {TokenKind.Equal}, but got {peek.Kind}.");
                            return null;
                        }

                        var right = TryParseExpression();

                        if (right == null)
                            return null;

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
                        return null;
                    }

                    leaf = new CompositTypeLiteral(leaf.Position) { Child = leaf, Values = assignments };
                    continue;
                }

                if (TryEatToken(TokenKind.ParenthesesLeft))
                {
                    var arguments = new List<Node>();
                    while (!TryEatToken(TokenKind.ParenthesesRight))
                    {
                        Token comma;
                        if (arguments.Count != 0 && !TryEatToken(TokenKind.Comma, out comma))
                        {
                            _compiler.ReportError(comma.Position,
                                $"Found an unexpected token when parsing a procedure call's arguments. Expected {TokenKind.Comma}, but got {comma.Kind}.");
                            return null;
                        }

                        var argument = TryParseExpression();
                        if (argument == null)
                            return null;

                        arguments.Add(argument);
                    }

                    leaf = new Call(leaf.Position) { Child = leaf, Arguments = arguments };
                    continue;
                }

                if (TryEatToken(TokenKind.SquareLeft))
                {
                    // TODO: implement indexing
                    _compiler.ReportError(leaf.Position, "Not Implemented");
                    return null;
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

        private UnaryNode TryParseUnaryOperatorOrArrayTypePrefix()
        {
            var first = PeekToken();

            if (IsUnaryOperator(first.Kind))
            {
                EatToken();
                return new UnaryOperator(first.Position, (UnaryOperatorKind)first.Kind);
            }

            if (PeekIs(TokenKind.SquareLeft))
            {
                var dimensions = 1;
                while (PeekIs(TokenKind.Comma, dimensions))
                    dimensions++;

                if (PeekIs(TokenKind.SquareLeft, dimensions))
                    return new ArrayPostfix(first.Position, dimensions);

                return null;
            }

            return null;
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
        /// <param name="result"></param>
        /// <returns></returns>
        private Node TryParseTerm()
        {
            Token start;
            if (TryEatToken(TokenKind.Identifier, out start))
                return new Symbol(start.Position, start.Value);

            if (TryEatToken(TokenKind.FloatNumber, out start))
                return new FloatLiteral(start.Position, double.Parse(start.Value));

            if (TryEatToken(TokenKind.DecimalNumber, out start))
                return new IntegerLiteral(start.Position, int.Parse(start.Value));
            
            if (TryEatToken(TokenKind.String, out start))
                return new StringLiteral(start.Position, start.Value);

            if (TryEatToken(TokenKind.ParenthesesLeft, out start))
            {
                var expression = TryParseExpression();
                if (expression == null)
                    return null;

                Token parRight;
                if (TryEatToken(TokenKind.ParenthesesRight, out parRight))
                    return expression;

                _compiler.ReportError(parRight.Position,
                    $"Could not find a pair for the left parentheses at {start.Position.Line}:{start.Position.Column}. Expected {TokenKind.ParenthesesRight}, but got {parRight.Kind}.");
                return null;
            }
            
            if (TryEatToken(TokenKind.KeywordStruct))
            {
                // TODO: implement indexing
                _compiler.ReportError(start.Position, "Not Implemented");
                return null;
            }

            // TODO: This does not parse correctly
            if (TryEatToken(TokenKind.KeywordFunction, out start) || TryEatToken(TokenKind.KeywordProcedure, out start))
            {
                Token peek;
                if (!TryEatToken(TokenKind.ParenthesesLeft, out peek))
                {
                    _compiler.ReportError(peek.Position,
                        $"Could not find the start parentheses at the start of a procedure's argument. Expected {TokenKind.ParenthesesLeft}, but got {peek.Kind}.");
                    return null;
                }

                // We can't determin from the arguments whether we are a procedure type or literal, if we have no arguments.
                // We therefore have to handle the empty procedure differently
                if (TryEatToken(TokenKind.ParenthesesRight))
                {
                    var returnType = TryParseExpression();
                    if (returnType == null)
                        return null;

                    if (TryEatToken(TokenKind.Arrow))
                    {
                        var block = TryParseCodeBlock();
                        if (block == null)
                            return null;

                        return new TypedProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                        {
                            Block = block,
                            Return = returnType,
                            Arguments = new Declaration[0]
                        };
                    }

                    return new ProcedureTypeNode(start.Position, start.Kind == TokenKind.KeywordFunction)
                    {
                        Return = returnType,
                        Arguments = new Node[0]
                    };
                }

                // If first argument looks like a declarations, then we parse the procedure as a literal
                if (PeekIs(TokenKind.Identifier) && PeekIs(TokenKind.Colon, 1))
                {
                    var arguments = new List<Declaration>();

                    while (!TryEatToken(TokenKind.ParenthesesRight))
                    {
                        Token comma;
                        if (arguments.Count != 0 && !TryEatToken(TokenKind.Comma, out comma))
                        {
                            _compiler.ReportError(comma.Position,
                                $"Found an unexpected token when parsing a procedure's arguments. Expected {TokenKind.Comma}, but got {comma.Kind}.");
                            return null;
                        }

                        var declaration = TryParseDeclaration();
                        if (declaration == null)
                            return null;

                        arguments.Add(declaration);
                    }

                    var returnType = TryParseExpression();
                    if (returnType == null)
                        return null;

                    Token arrow;
                    if (TryEatToken(TokenKind.Arrow, out arrow))
                    {
                        var block = TryParseCodeBlock();
                        if (block == null)
                            return null;

                        return new TypedProcedureLiteral(start.Position, start.Kind == TokenKind.KeywordFunction)
                        {
                            Block = block,
                            Return = returnType,
                            Arguments = arguments
                        };
                    }

                    // TODO: Error
                    _compiler.ReportError(arrow.Position, "");
                    return null;
                }

                // Else we parse it as a type
                {
                    var arguments = new List<Node>();

                    while (!TryEatToken(TokenKind.ParenthesesRight))
                    {
                        Token comma;
                        if (arguments.Count != 0 && !TryEatToken(TokenKind.Comma, out comma))
                        {
                            _compiler.ReportError(comma.Position,
                                $"Found an unexpected token when parsing a procedure's arguments. Expected {TokenKind.Comma}, but got {comma.Kind}.");
                            return null;
                        }

                        var argument = TryParseExpression();
                        if (argument == null)
                            return null;

                        arguments.Add(argument);
                    }

                    var returnType = TryParseExpression();
                    if (returnType == null)
                        return null;

                    return new ProcedureTypeNode(start.Position, start.Kind == TokenKind.KeywordFunction)
                    {
                        Arguments = arguments,
                        Return = returnType
                    };
                }
            }
            
            _compiler.ReportError(start.Position, 
                $"While parsing a term, the parser found an unexpected token: {start.Kind}");
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private CodeBlock TryParseCodeBlock()
        {
            Token curlyLeft;
            if (!TryEatToken(TokenKind.CurlyLeft, out curlyLeft))
            {
                _compiler.ReportError(curlyLeft.Position,
                    $"Did not find the start of a code block. Expected {TokenKind.CurlyLeft}, but got {curlyLeft.Kind}");
                return null;
            }

            var statements = new List<Node>();

            while (!TryEatToken(TokenKind.CurlyRight))
            {
                if (PeekIs(TokenKind.Identifier) && PeekIs(TokenKind.Colon, 1))
                {
                    var declaration = TryParseDeclaration();
                    if (declaration == null)
                        return null;

                    statements.Add(declaration);
                }
                else
                {
                    var expression = TryParseExpression();
                    if (expression == null)
                        return null;

                    statements.Add(expression);
                }
            }

            return new CodeBlock(curlyLeft.Position) { Statements = statements };
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

            var eaten = EatToken();
            Debug.Assert(eaten.Kind == result.Kind);
            return true;
        }

        /// <summary>
        /// Try to eat a token of a certain kind.
        /// </summary>
        /// <param name="expectedKind"></param>
        /// <returns></returns>
        private bool TryEatToken(TokenKind expectedKind)
        {
            Token peek;
            return TryEatToken(expectedKind, out peek);
        }

        private static readonly HashSet<BinaryOperatorKind> BinaryOperators = 
            new HashSet<BinaryOperatorKind>((BinaryOperatorKind[])Enum.GetValues(typeof(BinaryOperatorKind)));
        private static bool IsBinaryOperator(TokenKind kind) => BinaryOperators.Contains((BinaryOperatorKind)kind);
        
        private static readonly HashSet<UnaryOperatorKind> UnaryOperators =
            new HashSet<UnaryOperatorKind>((UnaryOperatorKind[])Enum.GetValues(typeof(UnaryOperatorKind)));
        private static bool IsUnaryOperator(TokenKind kind) => UnaryOperators.Contains((UnaryOperatorKind)kind);
    }
}