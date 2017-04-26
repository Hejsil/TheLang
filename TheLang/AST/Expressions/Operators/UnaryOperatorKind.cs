namespace TheLang.AST.Expressions.Operators
{
    public enum UnaryOperatorKind : byte
    {
        Negative = BinaryOperatorKind.Plus,
        Positive = BinaryOperatorKind.Minus,
        Not,
        Reference,
        UniqueReference,
        Dereference
    }
}
