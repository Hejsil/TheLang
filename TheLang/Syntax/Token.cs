namespace TheLang.Syntax
{
    public class Token
    {
        public Token(Position position, TokenKind kind, string value = null)
        {
            Position = position;
            Kind = kind;
            Value = value;
        }

        public TokenKind Kind { get; }
        public string Value { get; }
        public Position Position { get; }
    }
}
