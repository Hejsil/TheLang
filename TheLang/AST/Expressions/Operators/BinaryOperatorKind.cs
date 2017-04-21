namespace TheLang.AST.Expressions.Operators
{
    public enum BinaryOperatorKind : byte
    {
        // The first four bits represents the priority and assosiativity of that operator.
        // If the priority is even, then the associativity is Left-To-Right, else it is Right-To-Left.
        // The last four bits are used to make sure operators are uniquely represented.
        Dot = 0x10,

        As = 0x20,

        Times = 0x40,
        Divide = 0x41,
        Modulo = 0x42,

        Plus = 0x60,
        Minus = 0x61,

        LessThan = 0x80,
        LessThanEqual = 0x81,
        GreaterThan = 0x82,
        GreaterThanEqual = 0x83,

        Equal = 0xA0,
        NotEqual = 0xA1,

        And = 0xC0,
        Or = 0xC1,

        Assign = 0xD0,
        PlusAssign = 0xD1,
        MinusAssign = 0xD2,
        TimesAssign = 0xD3,
        DivideAssign = 0xD4,
        ModulusAssign = 0xD5
    }
}
