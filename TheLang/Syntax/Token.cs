namespace TheLang.Syntax
{
    public class Token
    {
        public Token(TokenKind kind, string value, Position position)
        {
            Kind = kind;
            Value = value;
            Position = position;
        }

        public TokenKind Kind { get; }
        public string Value { get; }
        public Position Position { get; }
    }
}
