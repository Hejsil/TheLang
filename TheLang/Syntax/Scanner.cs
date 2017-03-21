using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheLang.Syntax
{
    public class Scanner
    {
        private const char EndOfFile = '\0';
        private const char NewLine = '\n';

        private readonly Queue<Token> _tokenQueue = new Queue<Token>();

        private readonly string _program;
        private int _index = 0;

        private readonly string _fileName;
        private int _line = 0;
        private int _column = 0;

        private readonly Dictionary<string, TokenKind> _keywords = new Dictionary<string, TokenKind>()
        {
            { "as", TokenKind.KeywordAs },
            { "and", TokenKind.KeywordAnd },
            { "or", TokenKind.KeywordOr },
            { "struct", TokenKind.KeywordStruct },
            { "proc", TokenKind.KeywordProcedure },
            { "func", TokenKind.KeywordFunction },
        };

        public Scanner(string fileName)
            : this(File.OpenText(fileName))
        {
            _fileName = fileName;
        }

        public Scanner(TextReader stream)
        {
            _program = stream.ReadToEnd();
        }

        public Token Eat()
        {
            if (_tokenQueue.Count != 0)
                return _tokenQueue.Dequeue();

            return GetNextToken();
        }

        public Token Peek(int offset = 0)
        {
            while (_tokenQueue.Count <= offset)
                _tokenQueue.Enqueue(GetNextToken());

            return _tokenQueue.ElementAt(offset);
        }

        private Token GetNextToken()
        {
            SkipWhiteSpaceAndComments();

            var position = new Position(_fileName, _line, _column);
            var current = EatChar();
            var result = new StringBuilder(current);
            var peek = PeekChar();

            if (current == 'u' && peek == '@')
            {
                result.Append(EatChar());
                return new Token(TokenKind.UAt, result.ToString(), position);
            }

            if (current == 's' && peek == '@')
            {
                result.Append(EatChar());
                return new Token(TokenKind.SAt, result.ToString(), position);
            }

            if (char.IsLetter(current) || current == '_')
            {
                while (char.IsLetterOrDigit(peek) || peek == '_')
                {
                    result.Append(EatChar());
                    peek = PeekChar();
                }

                var resultStr = result.ToString();

                if (_keywords.TryGetValue(resultStr, out TokenKind kind))
                    return new Token(kind, resultStr, position);

                return new Token(TokenKind.Identifier, resultStr, position);
            }

            if (char.IsDigit(current))
            {
                while (char.IsDigit(peek) || peek == '_')
                {
                    result.Append(EatChar());
                    peek = PeekChar();
                }

                if (peek == '.')
                {
                    result.Append(EatChar());
                    while (char.IsDigit(peek) || peek == '_')
                    {
                        result.Append(EatChar());
                        peek = PeekChar();
                    }

                    return new Token(TokenKind.FloatNumber, result.ToString(), position);
                }

                return new Token(TokenKind.DecimalNumber, result.ToString(), position);
            }
            
            switch (current)
            {
                case '+':
                    return ScanSingleOrDoubleToken('=', TokenKind.PlusEqual, TokenKind.Plus, result, position);
                case '-':
                    if (peek == '>')
                    {
                        result.Append(EatChar());
                        return new Token(TokenKind.Arrow, result.ToString(), position);
                    }

                    return ScanSingleOrDoubleToken('=', TokenKind.MinusEqual, TokenKind.Minus, result, position);
                case '*':
                    return ScanSingleOrDoubleToken('=', TokenKind.TimesEqual, TokenKind.Times, result, position);
                case '/':
                    return ScanSingleOrDoubleToken('=', TokenKind.DivideEqual, TokenKind.Divide, result, position);
                case '%':
                    return ScanSingleOrDoubleToken('=', TokenKind.ModulusEqual, TokenKind.Modulo, result, position);
                case '^':
                    return ScanSingleOrDoubleToken('=', TokenKind.ExponentEqual, TokenKind.Exponent, result, position);
                case '=':
                    return ScanSingleOrDoubleToken('=', TokenKind.EqualEqual, TokenKind.Equal, result, position);
                case '<':
                    return ScanSingleOrDoubleToken('=', TokenKind.LessThanEqual, TokenKind.LessThan, result, position);
                case '>':
                    return ScanSingleOrDoubleToken('=', TokenKind.GreaterThanEqual, TokenKind.GreaterThan, result, position);
                case '@':
                    return new Token(TokenKind.At, result.ToString(), position);
                case '~':
                    return new Token(TokenKind.Tilde, result.ToString(), position);
                case '[':
                    return new Token(TokenKind.SquareLeft, result.ToString(), position);
                case ']':
                    return new Token(TokenKind.SquareRight, result.ToString(), position);
                case '(':
                    return new Token(TokenKind.ParenthesesLeft, result.ToString(), position);
                case ')':
                    return new Token(TokenKind.ParenthesesRight, result.ToString(), position);
                case '{':
                    return new Token(TokenKind.CurlyLeft, result.ToString(), position);
                case '}':
                    return new Token(TokenKind.CurlyRight, result.ToString(), position);
                case ':':
                    return new Token(TokenKind.Colon, result.ToString(), position);
                case ';':
                    return new Token(TokenKind.SemiColon, result.ToString(), position);
                case ',':
                    return new Token(TokenKind.Comma, result.ToString(), position);
                case '.':
                    if (char.IsDigit(peek))
                    {
                        result.Insert(0, '0');
                        result.Append(EatChar());
                        peek = PeekChar();

                        while (true)
                        {
                            if (char.IsDigit(peek))
                                result.Append(EatChar());
                            else if (peek == '_')
                                EatChar();
                            else
                                break;

                            peek = PeekChar();
                        }

                        return new Token(TokenKind.FloatNumber, result.ToString(), position);
                    }

                    return new Token(TokenKind.Dot, result.ToString(), position);
                case '!':
                    switch (peek)
                    {
                        case '=':
                            result.Append(EatChar());
                            return new Token(TokenKind.ExclamationMarkEqual, result.ToString(), position);
                        default:
                            return new Token(TokenKind.ExclamationMark, result.ToString(), position);
                    }
                case EndOfFile:
                    return new Token(TokenKind.EndOfFile, result.ToString(), position);
                default:
                    return new Token(TokenKind.Unknown, result.ToString(), position);
            }
        }

        private Token ScanSingleOrDoubleToken(char followChar, TokenKind match, TokenKind notMatch, StringBuilder result, Position position)
        {
            var peek = PeekChar();
            if (peek == followChar)
            {
                result.Append(EatChar());
                return new Token(match, result.ToString(), position);
            }

            return new Token(notMatch, result.ToString(), position);
        }

        private void SkipWhiteSpaceAndComments()
        {
            while (true)
            {
                var peek = PeekChar();

                if (char.IsWhiteSpace(peek))
                {
                    EatChar();
                }
                else if (peek == '/')
                {
                    peek = PeekChar();

                    switch (peek)
                    {
                        case '/':
                            while (EatChar() != NewLine) { }
                            break;
                        case '*':
                            EatChar();
                            var prev = EatChar();
                            var current = EatChar();

                            while (!(prev == '*' && current == '/'))
                            {
                                prev = current;
                                current = EatChar();
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private char PeekChar(int offset = 0)
        {
            if (_program.Length <= _index + offset)
                return EndOfFile;

            return _program[_index + offset];
        }

        private char EatChar()
        {
            if (_program.Length <= _index)
                return EndOfFile;

            var res = _program[_index];
            _index++;

            if (res == NewLine)
            {
                _line++;
                _column = 1;
            }
            else
            {
                _column++;
            }

            return res;
        }
    }
}
