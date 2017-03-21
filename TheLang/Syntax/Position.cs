namespace TheLang.Syntax
{
    public class Position
    {
        public Position(string fileName, int line, int column)
        {
            FileName = fileName;
            Line = line;
            Column = column;
        }

        public string FileName { get; }
        public int Line { get; }
        public int Column { get; }
    }
}
