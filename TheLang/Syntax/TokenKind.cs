using TheLang.AST.Expressions.Operators;

namespace TheLang.Syntax
{
    public enum TokenKind
    {
        Identifier,
        FloatNumber,
        DotFloatNumber,
        DecimalNumber,

        KeywordAs,
        KeywordStruct,
        KeywordAnd,
        KeywordOr,

        // Binary operators
        Plus = BinaryOperatorKind.Plus,
        Minus = BinaryOperatorKind.Minus,
        Times = BinaryOperatorKind.Times,
        Divide = BinaryOperatorKind.Divide,
        EqualEqual = BinaryOperatorKind.EqualEqual,
        Modulo = BinaryOperatorKind.Modulo,
        LessThan = BinaryOperatorKind.LessThan,
        LessThanEqual = BinaryOperatorKind.LessThanEqual,
        GreaterThan = BinaryOperatorKind.GreaterThan,
        GreaterThanEqual = BinaryOperatorKind.GreaterThanEqual,
        ExclamationMarkEqual = BinaryOperatorKind.ExclamationMarkEqual,

        // Assignements
        PlusEqual,
        MinusEqual,
        TimesEqual,
        DivideEqual,
        ModulusEqual,
        Equal,


        Exponent,
        ExponentEqual,

        ExclamationMark,

        Arrow,

        Not,

        At,
        SAt,
        UAt,
        
        Tilde,

        Dot,

        SquareLeft,
        SquareRight,

        ParenthesesLeft,
        ParenthesesRight,

        CurlyLeft,
        CurlyRight,

        Colon,
        SemiColon,
        Comma,

        Unknown,
        EndOfFile,
        KeywordProcedure,
        KeywordFunction,
    }
}
