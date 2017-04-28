namespace TheLang.Syntax
{
    public struct OpInfo
    {
        public OpInfo(int priority, Associativity associativity)
        {
            Priority = priority;
            Associativity = associativity;
        }

        public int Priority { get; }
        public Associativity Associativity { get; }
    }
}