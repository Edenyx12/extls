namespace extls.Core
{
    public static class Print
    {
        public static bool verbose = false;

        public static void Line(string message) => Console.WriteLine(message);
        public static void Line(string message, ConsoleColor color) => WriteLine(message, color);
        public static void Inline(string message) => Console.Write(message);
        public static void Inline(string message, ConsoleColor color) => Write(message, color);

        public static void Error(string message) => WriteLine(message, ConsoleColor.Red);
        public static void Warning(string message) => WriteLine(message, ConsoleColor.Yellow);
        public static void Info(string message) => WriteLine(message, ConsoleColor.Gray);

        public static void Debug(string message)
        {
            if (!verbose) return;

            Line(message, ConsoleColor.Cyan);
        }

        private static void WriteLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        private static void Write(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ResetColor();
        }
    }
}