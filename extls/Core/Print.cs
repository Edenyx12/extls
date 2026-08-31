using System.Drawing;

namespace extls.Core
{
    public static class Print
    {
        public static bool verbose = false;

        public static void Line(string message) => RawLine(message);
        public static void Line(string message, ConsoleColor color) => LineColor(message, color);
        public static void Inline(string message) => RawInline(message);
        public static void Inline(string message, ConsoleColor color) => InlineColor(message, color);

        public static void Error(string message) => LineColor(message, ConsoleColor.Red);
        public static void Warning(string message) => LineColor(message, ConsoleColor.Yellow);
        public static void Info(string message) => LineColor(message, ConsoleColor.Gray);

        public static void Debug(string message)
        {
            if (!verbose) return;

            Line(message, ConsoleColor.Cyan);
        }

        private static void LineColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            RawLine(message);
            Console.ResetColor();
        }
        private static void InlineColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            RawInline(message);
            Console.ResetColor();
        }

        private static void RawLine(string message)
        {
            Console.WriteLine(message);
            Console.Out.Flush();
        }
        private static void RawInline(string message)
        {
            Console.Write(message);
            Console.Out.Flush();
        }
    }
}