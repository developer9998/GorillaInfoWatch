using System.Text;

namespace GorillaInfoWatch.Utilities
{
    public static class AsciiUtils
    {
        public const string OpaqueSquare = "█", TranslucentSquare = "▒";

        public static string Bar(int length, int value)
        {
            StringBuilder str = new();
            for (int i = 0; i < length; i++)
            {
                string square = i < value ? OpaqueSquare : TranslucentSquare;
                str.Append(square);
            }
            return str.ToString();
        }
    }
}
