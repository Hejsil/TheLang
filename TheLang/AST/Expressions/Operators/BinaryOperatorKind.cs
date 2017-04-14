namespace TheLang.AST.Expressions.Operators
{
    public enum BinaryOperatorKind : byte
    {
        // The first four bits represents the priority of that operator.
        // The last four bits are used to make sure operators are uniquely represented.
        Dot = 0x00,

        As = 0x10,

        Times = 0x20,
        Divide = 0x21,
        Modulo = 0x22,

        Plus = 0x30,
        Minus = 0x31,

        LessThan = 0x40,
        LessThanEqual = 0x41,
        GreaterThan = 0x42,
        GreaterThanEqual = 0x43,

        Equal = 0x50,
        NotEqual = 0x51,

        And = 0x60,
        Or = 0x61
    }
}
