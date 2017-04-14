using TheLang.AST.Expressions.Operators;

namespace TheLang.Syntax
{
    public enum TokenKind
    {
        Identifier,
        FloatNumber,
        DecimalNumber,

        KeywordStruct,
        KeywordProcedure,
        KeywordFunction,

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

        // Unary operators
        ExclamationMark = UnaryOperatorKind.Not,
        At = UnaryOperatorKind.Reference,
        SAt = UnaryOperatorKind.SharedReference,
        UAt = UnaryOperatorKind.UniqueReference,
        Tilde = UnaryOperatorKind.Dereference,

        // Assignements
        PlusEqual,
        MinusEqual,
        TimesEqual,
        DivideEqual,
        ModulusEqual,
        Equal,
        
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
