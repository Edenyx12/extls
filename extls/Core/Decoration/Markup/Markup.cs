using System.Text;
using System.Runtime.CompilerServices;

namespace extls.Core.Decoration;

public enum MarkupTokenType {Text, Shield, Color, AutoColor, Bold, Italic}
public readonly record struct MarkupToken(string Token, MarkupTokenType Type);

public static partial class Markup
{
    static Markup() => System.Console.OutputEncoding = System.Text.Encoding.UTF8;

    public static void Rich(string code, bool lastWrap = false)
    {
        MarkupToken[] tokens = Parse(code);

        bool bold = false;
        bool italic = false;

        for (int i = 0; i < tokens.Length; i++)
        {
            switch (tokens[i].Type)
            {
                case MarkupTokenType.Text or MarkupTokenType.Shield: Out.Inline(tokens[i].Token); break;
                case MarkupTokenType.Italic:
                    italic = !italic;
                    Out.Inline(italic ? "\x1b[3m" : "\x1b[23m");
                    break;
                case MarkupTokenType.Bold:
                    bold = !bold;
                    Out.Inline(bold ? "\x1b[1m" : "\x1b[22m");
                    break;
                case MarkupTokenType.Color:
                    PrintColor(new Color(tokens[i].Token)); 
                    break;
                case MarkupTokenType.AutoColor:
                    if (!SetConsoleColor(tokens[i].Token)) {
                        Color c = ParseColor(tokens[i].Token);
                        Out.Inline(Color.ColorToConsoleFg(c));
                    } break;
            }
        }

        if (lastWrap) Console.WriteLine();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void PrintColor(Color c) => Out.Inline($"\x1b[38;2;{c.r};{c.g};{c.b}m");
    }

    private static MarkupToken[] Parse(string markup)
    {
        var tokens = new List<MarkupToken>();
        var raw = new StringBuilder();

        for (int i = 0; i < markup.Length; i++)
        {

            if (Match(markup, i, @"\"))      { AppendAndSave(ref i, 1, MarkupTokenType.Shield); continue; }
            else if (Match(markup, i, "**")) { AppendAndSave(ref i, 0, MarkupTokenType.Bold); continue; }
            else if (Match(markup, i, "*"))  { AppendAndSave(ref i, 0, MarkupTokenType.Italic); continue; }
            else if (Match(markup, i, "$"))
            {
                i++;

                if (Match(markup, i, "["))
                {
                    i++;

                    var c = new StringBuilder();

                    if (Match(markup, i, "#"))
                    {
                        i++;
                        AppendAndSave(ref i, 6, MarkupTokenType.Color);
                        continue;
                    }

                    int len = FindStopToken(markup, i, "]");
                    AppendAndSave(ref i, len, MarkupTokenType.AutoColor);
                }

                continue;
            }


            raw.Append(markup[i]);
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

            if (type is MarkupTokenType.Shield)
            {
                index++;
                raw.Append(markup[index]);
                index++;
            }
            else
            {
                int end = index + length;

                while (index < end)
                {
                    raw.Append(markup[index]);
                    index++;
                }
            }

            tokens.Add(new MarkupToken(raw.ToString(), type));
            raw.Clear();
        }

        return tokens.ToArray();
    }
    
    private static bool SetConsoleColor(ReadOnlySpan<char> name)
    {
        switch (name)
        {
            case "black":       Console.ForegroundColor = ConsoleColor.Black;       break;
            case "darkblue":    Console.ForegroundColor = ConsoleColor.DarkBlue;    break;
            case "darkgreen":   Console.ForegroundColor = ConsoleColor.DarkGreen;   break;
            case "darkcyan":    Console.ForegroundColor = ConsoleColor.DarkCyan;    break;
            case "darkred":     Console.ForegroundColor = ConsoleColor.DarkRed;     break;
            case "darkmagenta": Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
            case "darkyellow":  Console.ForegroundColor = ConsoleColor.DarkYellow;  break;
            case "gray":        Console.ForegroundColor = ConsoleColor.Gray;        break;
            case "darkgray":    Console.ForegroundColor = ConsoleColor.DarkGray;    break;
            case "blue":        Console.ForegroundColor = ConsoleColor.Blue;        break;
            case "green":       Console.ForegroundColor = ConsoleColor.Green;       break;
            case "cyan":        Console.ForegroundColor = ConsoleColor.Cyan;        break;
            case "red":         Console.ForegroundColor = ConsoleColor.Red;         break;
            case "magenta":     Console.ForegroundColor = ConsoleColor.Magenta;     break;
            case "yellow":      Console.ForegroundColor = ConsoleColor.Yellow;      break;
            case "white":       Console.ForegroundColor = ConsoleColor.White;       break;
            default:            return false;
        }
        return true;
    }

    private static Color ParseColor(ReadOnlySpan<char> name)
        => name switch {
        "nona"       => Color.Nona,
        "nonalux"    => Color.NonaLux,
        "octavus"    => Color.Octavus,
        "septima"    => Color.Septima,
        "sextus"     => Color.Sextus,
        "festive"    => Color.Festive,
        "genesis"    => Color.Genesis,
        "catppuccin" => Color.Catppuccin,
        "mint"       => Color.Mint,
        "purplerain" => Color.PurpleRain,
        "platypus"   => Color.Platypus,
        "barbie"     => Color.Barbie,
        "navy"       => Color.Navy,
        "cyanish"    => Color.Cyanish,
        "sevenup"    => Color.SevenUp,
        _            => new Color(255,255,255),
    };
}
