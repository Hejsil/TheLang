namespace TheLang.AST.Expressions.Operators
{
    public enum BinaryOperatorKind : byte
    {
        // The first four bits represents the priority of that operator.
        // The last four bits are used to make sure operators are uniquely represented.
        Dot = 0x00,

        Times = 0x10,
        Divide = 0x11,
        Modulo = 0x12,

        Plus = 0x20,
        Minus = 0x21,

        LessThan = 0x30,
        LessThanEqual = 0x31,
        GreaterThan = 0x32,
        GreaterThanEqual = 0x33,

        EqualEqual = 0x40,
        ExclamationMarkEqual = 0x41
    }
}
