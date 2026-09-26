using System.Drawing;

namespace extls.Core;

public enum OutType{Out, Err}

public static class Out
{
    public static void Line(string message, OutType type = OutType.Out) => OutLine(message, type);
    public static void Line(string message, ConsoleColor color, OutType type = OutType.Out) => LineColor(message, color, type);
    public static void Inline(string message, OutType type = OutType.Out) => OutInline(message, type);
    public static void Inline(string message, ConsoleColor color, OutType type = OutType.Out) => InlineColor(message, color, type);

    public static void Error(string message, OutType type = OutType.Out) => LineColor(message, ConsoleColor.Red, type);
    public static void Warning(string message, OutType type = OutType.Out) => LineColor(message, ConsoleColor.Yellow, type);
    public static void Info(string message, OutType type = OutType.Out) => LineColor(message, ConsoleColor.Gray, type);

    public static void Debug(string message)
    {
        if (!Global.Verbose) return;

        Line(message, ConsoleColor.Cyan);
    }

    private static void LineColor(string message, ConsoleColor color, OutType type)
    {
        Console.ForegroundColor = color;
        OutLine(message, type);
        Console.ResetColor();
    }
    private static void InlineColor(string message, ConsoleColor color, OutType type)
    {
        Console.ForegroundColor = color;
        OutInline(message, type);
        Console.ResetColor();
    }

    private static void OutLine(string message, OutType type)
    {
        switch (type)
        {
            case OutType.Out:
                Console.Out.WriteLine(message);
                Console.Out.Flush();
                break;
            case OutType.Err:
                Console.Error.WriteLine(message);
                Console.Error.Flush();
                break;
        }
    }

    private static void OutInline(string message, OutType type)
    {
        switch (type)
        {
            case OutType.Out:
                Console.Out.Write(message);
                Console.Out.Flush();
                break;
            case OutType.Err:
                Console.Error.Write(message);
                Console.Error.Flush();
                break;
        }
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