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
        private int _index = 0;

        private readonly string _fileName;
        private int _line = 1;
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
            stream.Dispose();
        }

        public Token EatToken()
        {
            if (_tokenQueue.Count != 0)
                return _tokenQueue.Dequeue();

            return GetNextToken();
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

            if (PeekIs('@', 1) && EatChar('u'))
            {
                Debug.Assert(EatChar('@'));
                return new Token(TokenKind.UAt, GetValue(startIndex), position);
            }

            if (EatChar(c => char.IsLetter(c) || c == '_') )
            {
                while (EatChar(c => char.IsLetterOrDigit(c) || c == '_')) { }

                var resultStr = GetValue(startIndex);

                TokenKind kind;
                if (_keywords.TryGetValue(resultStr, out kind))
                    return new Token(kind, resultStr, position);

                return new Token(TokenKind.Identifier, resultStr, position);
            }

            if (EatChar(char.IsDigit))
            {
                while (EatChar(c => char.IsDigit(c) || c == '_')) { }

                if (!EatChar('.'))
                    return new Token(TokenKind.DecimalNumber, GetValue(startIndex), position);

                while (EatChar(c => char.IsDigit(c) || c == '_')) { }

                return new Token(TokenKind.FloatNumber, GetValue(startIndex), position);
            }

            var eaten = PeekChar();
            EatChar();
            switch (eaten)
            {
                case '+':
                    return new Token(EatChar('=') ? TokenKind.PlusEqual : TokenKind.Plus, GetValue(startIndex), position);
                case '-':
                    return new Token(EatChar('=') ? TokenKind.MinusEqual : TokenKind.Minus, GetValue(startIndex), position);
                case '*':
                    return new Token(EatChar('=') ? TokenKind.TimesEqual : TokenKind.Times, GetValue(startIndex), position);
                case '/':
                    return new Token(EatChar('=') ? TokenKind.DivideEqual : TokenKind.Divide, GetValue(startIndex), position);
                case '%':
                    return new Token(EatChar('=') ? TokenKind.ModulusEqual : TokenKind.Modulo, GetValue(startIndex), position);
                case '^':
                    return new Token(EatChar('=') ? TokenKind.ExponentEqual : TokenKind.Exponent, GetValue(startIndex), position);
                case '=':
                    if (EatChar('>'))
                        return new Token(TokenKind.Arrow, GetValue(startIndex), position);

                    return new Token(EatChar('=') ? TokenKind.EqualEqual : TokenKind.Equal, GetValue(startIndex), position);
                case '<':
                    return new Token(EatChar('=') ? TokenKind.LessThanEqual : TokenKind.LessThan, GetValue(startIndex), position);
                case '>':
                    return new Token(EatChar('=') ? TokenKind.GreaterThanEqual : TokenKind.GreaterThan, GetValue(startIndex), position);
                case '!':
                    return new Token(EatChar('=') ? TokenKind.ExclamationMarkEqual : TokenKind.ExclamationMark, GetValue(startIndex), position);
                case '@':
                    return new Token(TokenKind.At, GetValue(startIndex), position);
                case '~':
                    return new Token(TokenKind.Tilde, GetValue(startIndex), position);
                case '[':
                    return new Token(TokenKind.SquareLeft, GetValue(startIndex), position);
                case ']':
                    return new Token(TokenKind.SquareRight, GetValue(startIndex), position);
                case '(':
                    return new Token(TokenKind.ParenthesesLeft, GetValue(startIndex), position);
                case ')':
                    return new Token(TokenKind.ParenthesesRight, GetValue(startIndex), position);
                case '{':
                    return new Token(TokenKind.CurlyLeft, GetValue(startIndex), position);
                case '}':
                    return new Token(TokenKind.CurlyRight, GetValue(startIndex), position);
                case ':':
                    return new Token(TokenKind.Colon, GetValue(startIndex), position);
                case ';':
                    return new Token(TokenKind.SemiColon, GetValue(startIndex), position);
                case ',':
                    return new Token(TokenKind.Comma, GetValue(startIndex), position);
                case '.':
                    if (!EatChar(char.IsDigit))
                        return new Token(TokenKind.Dot, GetValue(startIndex), position);

                    while (EatChar(c => char.IsDigit(c) || c == '_')) { }

                    return new Token(TokenKind.FloatNumber, $"0{GetValue(startIndex).Replace("_", "")}", position);

                case EndOfFile:
                    return new Token(TokenKind.EndOfFile, GetValue(startIndex), position);
                case '"':
                    while (!EatChar('"'))
                    {
                        if (PeekIs(EndOfFile))
                            return new Token(TokenKind.Unknown, GetValue(startIndex), position);

                        EatChar('\\');
                        EatChar();
                    }

                    return new Token(TokenKind.String, GetValue(startIndex + 1, -1), position);
                default:
                    return new Token(TokenKind.Unknown, GetValue(startIndex), position);
            }
        }

        private void SkipWhiteSpaceAndComments()
        {
            for (;;)
            {
                if (EatChar(char.IsWhiteSpace))
                    continue;

                if (PeekIs('/'))
                {
                    if (PeekIs('/', 1))
                    {
                        Debug.Assert(EatChar('/'));
                        Debug.Assert(EatChar('/'));
                        while (EatChar(c => c != NewLine)) { }
                        continue;
                    }

                    if (PeekIs('*', 1))
                    {
                        Debug.Assert(EatChar('/'));
                        Debug.Assert(EatChar('*'));

                        while (!(PeekIs('*') && PeekIs('/')))
                            EatChar();

                        Debug.Assert(EatChar('*'));
                        Debug.Assert(EatChar('/'));
                        continue;
                    }
                }

                break;
            }
        }

        private bool EatChar(Predicate<char> predicate) => EatChar(PeekIs(predicate));
        private bool EatChar(char chr) => EatChar(PeekIs(chr));

        private bool PeekIs(Predicate<char> predicate, int offset = 0) => predicate(PeekChar(offset));
        private bool PeekIs(char predicate, int offset = 0) => predicate == PeekChar(offset);

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
