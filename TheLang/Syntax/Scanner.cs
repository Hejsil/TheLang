using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TheLang.Syntax
{
    public class Scanner
    {
        private const char EndOfFile = '\0';
        private const char NewLine = '\n';

        private readonly Queue<Token> _tokenQueue = new Queue<Token>();

        private readonly string _program;
        private int _index;

        private readonly string _fileName;
        private int _line = 1;
        private int _column;

        private readonly Dictionary<string, TokenKind> _keywords = new Dictionary<string, TokenKind>()
        {
            { "as", TokenKind.KeywordAs },
            { "and", TokenKind.KeywordAnd },
            { "or", TokenKind.KeywordOr },
            { "struct", TokenKind.KeywordStruct },
            { "proc", TokenKind.KeywordProcedure },
            { "func", TokenKind.KeywordFunction },
            { "return", TokenKind.KeywordReturn },
        };

        public Scanner(string fileName)
            : this(File.OpenText(fileName))
        {
            _fileName = fileName;
        }

        public Scanner(TextReader stream)
        {
            _program = stream.ReadToEnd();
            stream.Dispose();
        }

        public Token EatenToken { get; private set; }

        public bool PeekTokenIs(TokenKind expectedKind, int offset = 0) => PeekToken(offset).Kind == expectedKind;
        public bool PeekTokenIs(Predicate<Token> predicate, int offset = 0) => predicate(PeekToken(offset));
        public bool PeekTokenIs(Predicate<TokenKind> predicate, int offset = 0) => predicate(PeekToken(offset).Kind);

        public bool EatToken(TokenKind expected) => EatToken(PeekTokenIs(expected));
        public bool EatToken(Predicate<Token> expected) => EatToken(expected(PeekToken()));
        public bool EatToken(Predicate<TokenKind> expected) => EatToken(expected(PeekToken().Kind));

        public bool EatToken(bool eat = true)
        {
            if (!eat)
                return false;

            EatenToken = _tokenQueue.Count == 0 ? GetNextToken() : _tokenQueue.Dequeue();
            return true;
        }

        public Token PeekToken(int offset = 0)
        {
            while (_tokenQueue.Count <= offset)
                _tokenQueue.Enqueue(GetNextToken());

            return _tokenQueue.ElementAt(offset);
        }

        private Token GetNextToken()
        {
            SkipWhiteSpaceAndComments();

            var position = new Position(_fileName, _line, _column);
            var startIndex = _index;

            if (PeekCharIs('@', 1) && EatChar('u'))
            {
                Debug.Assert(EatChar('@'));
                return new Token(position, TokenKind.UAt);
            }

            if (EatChar(c => char.IsLetter(c) || c == '_') )
            {
                while (EatChar(c => char.IsLetterOrDigit(c) || c == '_')) { }

                var resultStr = GetValue(startIndex);

                TokenKind kind;
                if (_keywords.TryGetValue(resultStr, out kind))
                    return new Token(position, kind);

                return new Token(position, TokenKind.Identifier, resultStr);
            }

            if (EatChar(char.IsDigit))
            {
                while (EatChar(c => char.IsDigit(c) || c == '_')) { }

                if (!EatChar('.'))
                    return new Token(position, TokenKind.DecimalNumber, GetValue(startIndex).Replace("_", ""));

                while (EatChar(c => char.IsDigit(c) || c == '_')) { }

                return new Token(position, TokenKind.FloatNumber, GetValue(startIndex).Replace("_", ""));
            }

            var eaten = PeekChar();
            EatChar();
            switch (eaten)
            {
                case '+':
                    return new Token(position, EatChar('=') ? TokenKind.PlusEqual : TokenKind.Plus);
                case '-':
                    return new Token(position, EatChar('=') ? TokenKind.MinusEqual : TokenKind.Minus);
                case '*':
                    return new Token(position, EatChar('=') ? TokenKind.TimesEqual : TokenKind.Times);
                case '/':
                    return new Token(position, EatChar('=') ? TokenKind.DivideEqual : TokenKind.Divide);
                case '%':
                    return new Token(position, EatChar('=') ? TokenKind.ModulusEqual : TokenKind.Modulo);
                case '^':
                    return new Token(position, EatChar('=') ? TokenKind.ExponentEqual : TokenKind.Exponent);
                case '=':
                    if (EatChar('>'))
                        return new Token(position, TokenKind.Arrow);

                    return new Token(position, EatChar('=') ? TokenKind.EqualEqual : TokenKind.Equal);
                case '<':
                    return new Token(position, EatChar('=') ? TokenKind.LessThanEqual : TokenKind.LessThan);
                case '>':
                    return new Token(position, EatChar('=') ? TokenKind.GreaterThanEqual : TokenKind.GreaterThan);
                case '!':
                    return new Token(position, EatChar('=') ? TokenKind.ExclamationMarkEqual : TokenKind.ExclamationMark);
                case '@':
                    return new Token(position, TokenKind.At);
                case '~':
                    return new Token(position, TokenKind.Tilde);
                case '[':
                    return new Token(position, TokenKind.SquareLeft);
                case ']':
                    return new Token(position, TokenKind.SquareRight);
                case '(':
                    return new Token(position, TokenKind.ParenthesesLeft);
                case ')':
                    return new Token(position, TokenKind.ParenthesesRight);
                case '{':
                    return new Token(position, TokenKind.CurlyLeft);
                case '}':
                    return new Token(position, TokenKind.CurlyRight);
                case ':':
                    return new Token(position, TokenKind.Colon);
                case ';':
                    return new Token(position, TokenKind.SemiColon);
                case ',':
                    return new Token(position, TokenKind.Comma);
                case '.':
                    if (!EatChar(char.IsDigit))
                        return new Token(position, TokenKind.Dot);

                    while (EatChar(c => char.IsDigit(c) || c == '_')) { }

                    return new Token(position, TokenKind.FloatNumber, $"0{GetValue(startIndex).Replace("_", "")}");

                case EndOfFile:
                    return new Token(position, TokenKind.EndOfFile);
                case '"':
                    while (!EatChar('"'))
                    {
                        if (PeekCharIs(EndOfFile))
                            return new Token(position, TokenKind.Unknown, GetValue(startIndex));

                        EatChar('\\');
                        EatChar();
                    }

                    return new Token(position, TokenKind.String, GetValue(startIndex + 1, -1));
                default:
                    return new Token(position, TokenKind.Unknown, GetValue(startIndex));
            }
        }

        private void SkipWhiteSpaceAndComments()
        {
            for (;;)
            {
                if (EatChar(char.IsWhiteSpace))
                    continue;

                if (PeekCharIs('/'))
                {
                    if (PeekCharIs('/', 1))
                    {
                        Debug.Assert(EatChar('/'));
                        Debug.Assert(EatChar('/'));
                        while (EatChar(c => c != NewLine)) { }
                        continue;
                    }

                    if (PeekCharIs('*', 1))
                    {
                        Debug.Assert(EatChar('/'));
                        Debug.Assert(EatChar('*'));

                        while (!(PeekCharIs('*') && PeekCharIs('/')))
                            EatChar();

                        Debug.Assert(EatChar('*'));
                        Debug.Assert(EatChar('/'));
                        continue;
                    }
                }

                break;
            }
        }

        private bool EatChar(Predicate<char> predicate) => EatChar(PeekCharIs(predicate));
        private bool EatChar(char chr) => EatChar(PeekCharIs(chr));

        private bool PeekCharIs(Predicate<char> predicate, int offset = 0) => predicate(PeekChar(offset));
        private bool PeekCharIs(char predicate, int offset = 0) => predicate == PeekChar(offset);

        private char PeekChar(int offset = 0)
        {
            if (_program.Length <= _index + offset)
                return EndOfFile;

            return _program[_index + offset];
        }

        private bool EatChar(bool eat = true)
        {
            if (!eat || _program.Length <= _index)
                return false;

            var res = _program[_index];
            _index++;

            if (res == NewLine)
            {
                _line++;
                _column = 0;
                return true;
            }

             _column++;
            return true;
        }

        private string GetValue(int startIndex, int indexOffset = 0) => _program.Substring(startIndex, (_index + indexOffset) - startIndex);
    }
}
