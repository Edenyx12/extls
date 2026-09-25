namespace extls.Core.Decoration;

public static partial class Markup
{
    public static void Line() => Line(3,40,2);
    public static void Line(int minimum = 3, int preferred = 40, int indent = 2, char line = '─')
    {
        int max  = Console.WindowWidth - (indent * 2);
        int pmax = preferred + (indent * 2);
        int w    = preferred;

        if (pmax <= Console.WindowWidth)     w = pmax;
        else if (pmax > Console.WindowWidth) w = max;

        if (w < minimum) w = minimum;

        string strIndent = new string(' ', indent);
        string strLine   = new string(line, w);

        Out.Line(strIndent + strLine + strIndent);
    }
    public static void Line(Color c, int minimum = 3, int preferred = 40, int indent = 2, char line = '─')
    {
        int max  = Console.WindowWidth - (indent * 2);
        int pmax = preferred + (indent * 2);
        int w    = preferred;

        if (pmax <= Console.WindowWidth)     w = pmax;
        else if (pmax > Console.WindowWidth) w = max;

        if (w < minimum) w = minimum;

        string strIndent = new string(' ', indent);
        string strLine   = new string(line, w);
        string color     = Color.ColorToConsoleFg(c);

        Out.Line(color + strIndent + strLine + strIndent);
        Console.ResetColor();
    }
    public static void Line(ConsoleColor c, int minimum = 3, int preferred = 40, int indent = 2, char line = '─')
    {
        int max  = Console.WindowWidth - (indent * 2);
        int pmax = preferred + (indent * 2);
        int w    = preferred;

        if (pmax <= Console.WindowWidth)     w = pmax;
        else if (pmax > Console.WindowWidth) w = max;

        if (w < minimum) w = minimum;

        string strIndent = new string(' ', indent);
        string strLine   = new string(line, w);

        Console.ForegroundColor = c;
        Out.Line(strIndent + strLine + strIndent);
        Console.ResetColor();
    }
    public static void Line(Gradient g, int minimum = 3, int preferred = 40, int indent = 2, char line = '─')
    {
        int max  = Console.WindowWidth - (indent * 2);
        int pmax = preferred + (indent * 2);
        int w    = preferred;

        if (pmax <= Console.WindowWidth)     w = pmax;
        else if (pmax > Console.WindowWidth) w = max;

        if (w < minimum) w = minimum;

        string strIndent = new string(' ', indent);
        string strLine   = g.PaintString(new string(line, w));

        Out.Line(strIndent + strLine + strIndent);
        Console.ResetColor();
    }
}