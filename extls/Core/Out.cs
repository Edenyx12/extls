using System.Drawing;

namespace extls.Core;

public static class Out
{
    public static void Line(string message) => RawLine(message);
    public static void Line(string message, ConsoleColor color) => LineColor(message, color);
    public static void Inline(string message) => RawInline(message);
    public static void Inline(string message, ConsoleColor color) => InlineColor(message, color);

    public static void Error(string message) => LineColor(message, ConsoleColor.Red);
    public static void Warning(string message) => LineColor(message, ConsoleColor.Yellow);
    public static void Info(string message) => LineColor(message, ConsoleColor.Gray);

    public static void Debug(string message)
    {
        if (!Global.Verbose) return;

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

    public static void EnableAlternateBuffer()
    {
        Console.Write("\x1b[?1049h");
        Console.SetCursorPosition(0, 0); 
    }

    public static void DisableAlternateBuffer()
    {
        Console.Write("\x1b[?1049l");
    }
}