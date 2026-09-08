using System.Text;

namespace extls.Core.Decoration;

public enum MarkupTokenType { Text, Shield, Color, AutoColor, Bold, Italic }
public readonly record struct MarkupToken(string Token, MarkupTokenType Type);

public static class Markup
{
    static Markup()
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;
    }

    public static void Rich(string code, bool lastWrap = false)
    {
        MarkupToken[] tokens = Parse(code);

        bool bold = false;
        bool italic = false;

        for (int i = 0; i < tokens.Length; i++)
        {
            switch (tokens[i].Type)
            {
                case MarkupTokenType.Text: Print.Inline(tokens[i].Token); break;
                case MarkupTokenType.Shield: Print.Inline(tokens[i].Token); break;
                case MarkupTokenType.Italic:
                    if (italic) {
                        italic = false;
                        Print.Inline("\x1b[23m");
                        break;
                    }
                    else {
                        italic = true;
                        Print.Inline("\x1b[3m");
                        break;
                    }
                case MarkupTokenType.Bold:
                    if (bold) {
                        bold = false;
                        Print.Inline("\x1b[22m");
                        break;
                    }
                    else {
                        bold = true;
                        Print.Inline("\x1b[1m");
                        break;
                    }
                case MarkupTokenType.Color:
                    Color color = Color.HEXToColor(tokens[i].Token);
                    Print.Inline($"\x1b[38;2;{color.r};{color.g};{color.b}m");
                    break;
                case MarkupTokenType.AutoColor:
                    SetConsoleColor(tokens[i].Token);
                    break;
            }
        }

        if (lastWrap) Console.WriteLine();
    }

    public static string FixBackslash(string text)
    {
        int index = text.IndexOf('\\');

        if (index < 0) return text;

        var fix = new StringBuilder(text.Length + 1);

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] is '\\')
                fix.Append('\\');

            fix.Append(text[i]);
        }

        return fix.ToString();
    }

    private static MarkupToken[] Parse(string code)
    {
        var tokens = new List<MarkupToken>();
        var raw = new StringBuilder();

        for (int i = 0; i < code.Length; i++)
        {

            if (Match(code, i, "\\")) { AppendAndSave(ref i, 1, MarkupTokenType.Shield); continue; }
            else if (Match(code, i, "**")) { AppendAndSave(ref i, 0, MarkupTokenType.Bold); continue; }
            else if (Match(code, i, "*")) { AppendAndSave(ref i, 0, MarkupTokenType.Italic); continue; }
            else if (Match(code, i, "$"))
            {
                i++;

                if (Match(code, i, "["))
                {
                    i++;

                    var c = new StringBuilder();

                    if (Match(code, i, "#"))
                    {
                        i++;
                        AppendAndSave(ref i, 6, MarkupTokenType.Color);
                        continue;
                    }

                    int len = FindStopToken(code, i, "]");
                    AppendAndSave(ref i, len, MarkupTokenType.AutoColor);
                }

                continue;
            }


            raw.Append(code[i]);
        }

        if (raw.Length > 0)
        {
            tokens.Add(new MarkupToken(raw.ToString(), MarkupTokenType.Text));
            raw.Clear();
        }

        void AppendAndSave(ref int index, int length, MarkupTokenType type)
        {
            if (raw.Length > 0)
            {
                tokens.Add(new MarkupToken(raw.ToString(), MarkupTokenType.Text));
                raw.Clear();
            }

            int end = index + length;

            while (index < end)
            {
                raw.Append(code[index]);
                index++;
            }

            tokens.Add(new MarkupToken(raw.ToString(), type));
            raw.Clear();
        }

        return tokens.ToArray();
    }
    private static bool Match(string str, int index, string target)
    {
        if (index < 0 || index + target.Length > str.Length) return false;

        ReadOnlySpan<char> slice = str.AsSpan(index, target.Length);
        return slice.SequenceEqual(target);
    }
    private static int FindStopToken(string str, int index, string stopToken)
    {
        int stop = str.IndexOf(stopToken, index, StringComparison.Ordinal);
        return stop == -1 ? -1 : stop - index;
    }
    private static void SetConsoleColor(string color)
    {
        Console.ForegroundColor = color.ToLowerInvariant() switch
        {
            "black" => ConsoleColor.Black,
            "darkblue" => ConsoleColor.DarkBlue,
            "darkgreen" => ConsoleColor.DarkGreen,
            "darkcyan" => ConsoleColor.DarkCyan,
            "darkred" => ConsoleColor.DarkRed,
            "darkmagenta" => ConsoleColor.DarkMagenta,
            "darkyellow" => ConsoleColor.DarkYellow,
            "gray" => ConsoleColor.Gray,
            "darkgray" => ConsoleColor.DarkGray,
            "blue" => ConsoleColor.Blue,
            "green" => ConsoleColor.Green,
            "cyan" => ConsoleColor.Cyan,
            "red" => ConsoleColor.Red,
            "magenta" => ConsoleColor.Magenta,
            "yellow" => ConsoleColor.Yellow,
            _ or "white" => ConsoleColor.White
        };
    }
}
