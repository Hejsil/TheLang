namespace TheLang.Syntax
{
    public enum TokenKind
    {
        // Unary operators
        ExclamationMark,
        UAt,
        At,
        Tilde,
        And,

        // Binary operators
        Plus,
        Minus,
        Times,
        Divide,
        EqualEqual,
        Modulo,
        LessThan,
        LessThanEqual,
        GreaterThan,
        GreaterThanEqual,
        ExclamationMarkEqual,
        KeywordAnd,
        KeywordOr,
        KeywordAs,
        Dot,

        Equal,
        PlusEqual,
        MinusEqual,
        TimesEqual,
        DivideEqual,
        ModulusEqual,

        Identifier,
        CompilerIdentifier,
        FloatNumber,
        DecimalNumber,
        String,

        KeywordStruct,
        KeywordProcedure,
        KeywordFunction,
        KeywordReturn,
        KeywordVar,
        KeywordConst,
        KeywordEnum,
        KeywordBreak,
        KeywordContinue,

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
        Unknown
    }
}
