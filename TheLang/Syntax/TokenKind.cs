using TheLang.AST.Expressions.Operators;

namespace TheLang.Syntax
{
    public enum TokenKind
    {
        // Unary operators
        ExclamationMark = UnaryOperatorKind.Not,
        At = UnaryOperatorKind.Reference,
        UAt = UnaryOperatorKind.UniqueReference,
        Tilde = UnaryOperatorKind.Dereference,

        // Binary operators
        Plus = BinaryOperatorKind.Plus,
        Minus = BinaryOperatorKind.Minus,
        Times = BinaryOperatorKind.Times,
        Divide = BinaryOperatorKind.Divide,
        EqualEqual = BinaryOperatorKind.Equal,
        Modulo = BinaryOperatorKind.Modulo,
        LessThan = BinaryOperatorKind.LessThan,
        LessThanEqual = BinaryOperatorKind.LessThanEqual,
        GreaterThan = BinaryOperatorKind.GreaterThan,
        GreaterThanEqual = BinaryOperatorKind.GreaterThanEqual,
        ExclamationMarkEqual = BinaryOperatorKind.NotEqual,
        KeywordAnd = BinaryOperatorKind.And,
        KeywordOr = BinaryOperatorKind.Or,
        KeywordAs = BinaryOperatorKind.As,
        Dot = BinaryOperatorKind.Dot,
        Equal = BinaryOperatorKind.Assign,
        PlusEqual = BinaryOperatorKind.PlusAssign,
        MinusEqual = BinaryOperatorKind.MinusAssign,
        TimesEqual = BinaryOperatorKind.TimesAssign,
        DivideEqual = BinaryOperatorKind.DivideAssign,
        ModulusEqual = BinaryOperatorKind.ModulusAssign,

        Identifier,
        FloatNumber,
        DecimalNumber,
        String,

        KeywordStruct,
        KeywordProcedure,
        KeywordFunction,

        Exponent,
        ExponentEqual,

        Arrow,

        SquareLeft,
        SquareRight,

        ParenthesesLeft,
        ParenthesesRight,

        CurlyLeft,
        CurlyRight,

        Colon,
        SemiColon,
        Comma,

        EndOfFile,
        Unknown,
    }
}
