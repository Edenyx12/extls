using System.Text;
using System.Runtime.CompilerServices;

namespace extls.Core.Decoration;

public enum MarkupTokenType {Text, Shield, Color, AutoColor, Bold, Italic, GradientStart, GradientEnd}
public readonly record struct MarkupToken(string Token, MarkupTokenType Type);

public static partial class Markup
{
    public static string boldOpen = "\x1b[1m";
    public static string boldClose = "\x1b[22m";
    public static string italicOpen = "\x1b[3m";
    public static string italicClose = "\x1b[23m";

    private static StringBuilder richBuilder = new();
    private static StringBuilder gradientBuilder = new();
    private static Gradient gradient;

    static Markup() => System.Console.OutputEncoding = System.Text.Encoding.UTF8;

    public static void Rich(string code, bool lastWrap = false, OutType type = OutType.Out)
    {
        richBuilder.Clear();
        gradientBuilder.Clear();

        MarkupToken[] tokens = Parse(code);

        bool bold = false;
        bool italic = false;
        bool gradient = false;

        for (int i = 0; i < tokens.Length; i++)
        {
            switch (tokens[i].Type)
            {
                case MarkupTokenType.Text or MarkupTokenType.Shield: 
                    if (gradient) gradientBuilder.Append(tokens[i].Token);
                    else          richBuilder.Append(tokens[i].Token);
                    break;
                case MarkupTokenType.Italic:
                    if (Console.IsOutputRedirected) break;

                    italic = !italic;

                    if (gradient) gradientBuilder.Append(italic ? italicOpen : italicClose);
                    else          richBuilder.Append(italic ? italicOpen : italicClose);
                    break;
                case MarkupTokenType.Bold:
                    if (Console.IsOutputRedirected) break;

                    bold = !bold;

                    if (gradient) gradientBuilder.Append(bold ? boldOpen : boldClose);
                    else          richBuilder.Append(bold ? boldOpen : boldClose);
                    break;
                case MarkupTokenType.Color:
                    if (Console.IsOutputRedirected) break;

                    if (gradient)
                    {
                        PaintGradientBuilder();
                        gradient = false;
                    }

                    richBuilder.Append(Color.ColorToConsoleFg(tokens[i].Token));
                    break;
                case MarkupTokenType.AutoColor:
                    if (Console.IsOutputRedirected) break;

                    if (gradient)
                    {
                        PaintGradientBuilder();
                        gradient = false;
                    }

                    richBuilder.Append(Color.ColorToConsoleFg(ParseColor(tokens[i].Token)));
                    break;
                case MarkupTokenType.GradientStart:
                    gradient = true;
                    Markup.gradient = GetGradient(tokens[i].Token);
                    break;
                case MarkupTokenType.GradientEnd:
                    gradient = false;
                    PaintGradientBuilder();
                    break;
            }
        }

        if (gradient) PaintGradientBuilder();
        if (bold) richBuilder.Append(boldClose);
        if (italic) richBuilder.Append(italicClose);

        richBuilder.Append(Color.ColorToConsoleFg(new Color(0xff,0xff,0xff)));

        Out.Inline(richBuilder.ToString(), type);
        if (lastWrap) Console.WriteLine();
    }

    private static MarkupToken[] Parse(ReadOnlySpan<char> markup)
    {
        var tokens = new List<MarkupToken>();
        var raw = new StringBuilder();

        for (int i = 0; i < markup.Length; i++)
        {

            if (Match(markup, i, @"\"))
            {
                AppendAndSave(ref i, 1, MarkupTokenType.Shield, ref markup);
                continue;
            }
            else if (Match(markup, i, "**"))
            {
                AppendAndSave(ref i, 1, MarkupTokenType.Bold, ref markup);
                continue;
            }
            else if (Match(markup, i, "*"))
            {
                AppendAndSave(ref i, 0, MarkupTokenType.Italic, ref markup);
                continue;
            }
            else if (Match(markup, i, "$"))
            {
                i++;

                if (Match(markup, i, "["))
                {
                    i++;

                    if (Match(markup, i, "#"))
                    {
                        i++;
                        AppendAndSave(ref i, 6, MarkupTokenType.Color, ref markup);
                        continue;
                    }

                    int len = FindStopToken(markup, i, "]");
                    AppendAndSave(ref i, len, MarkupTokenType.AutoColor, ref markup);
                }
                else if (Match(markup, i, "g"))
                {
                    i++;

                    if (Match(markup, i, "["))
                    {
                        i++;
                        int len = FindStopToken(markup, i, "]");
                        AppendAndSave(ref i, len, MarkupTokenType.GradientStart, ref markup);
                    }
                    else AppendAndSave(ref i, 0, MarkupTokenType.GradientEnd, ref markup);
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

        void AppendAndSave(ref int index, int length, MarkupTokenType type, ref ReadOnlySpan<char> mrkp)
        {
            if (raw.Length > 0)
            {
                tokens.Add(new MarkupToken(raw.ToString(), MarkupTokenType.Text));
                raw.Clear();
            }

            if (type is MarkupTokenType.Shield)
            {
                if (index + 1 < mrkp.Length)
                {
                    index++;
                    raw.Append(mrkp[index]);
                }
            }
            else
            {
                int end = index + length;

                while (index < end)
                {
                    raw.Append(mrkp[index]);
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
    private static bool ParseColor(ReadOnlySpan<char> name, out Color color)
    {
        switch (name)
        {
            case "nona":       color = Color.Nona;             return true;
            case "nonalux":    color = Color.NonaLux;          return true;
            case "octavus":    color = Color.Octavus;          return true;
            case "septima":    color = Color.Septima;          return true;
            case "sextus":     color = Color.Sextus;           return true;
            case "festive":    color = Color.Festive;          return true;
            case "genesis":    color = Color.Genesis;          return true;
            case "catppuccin": color = Color.Catppuccin;       return true;
            case "mint":       color = Color.Mint;             return true;
            case "purplerain": color = Color.PurpleRain;       return true;
            case "platypus":   color = Color.Platypus;         return true;
            case "barbie":     color = Color.Barbie;           return true;
            case "navy":       color = Color.Navy;             return true;
            case "cyanish":    color = Color.Cyanish;          return true;
            case "sevenup":    color = Color.SevenUp;          return true;
            default:           color = new Color(255,255,255); return false;
        }
    }
    private static Gradient GetGradient(ReadOnlySpan<char> colors)
    {
        int index = FindStopToken(colors, 0, ",");

        ReadOnlySpan<char> first = colors[..index];
        ReadOnlySpan<char> second = colors[(index + 1)..];

        Color color1 = ParseColor(first, out Color parsed1)
            ? parsed1
            : Color.HEXToColorSafe(first);

        Color color2 = ParseColor(second, out Color parsed2)
            ? parsed2
            : Color.HEXToColorSafe(second);

        return new Gradient(color1, color2);
    }
    private static void PaintGradientBuilder()
    {
        richBuilder.Append(gradient.PaintString(gradientBuilder.ToString()));
        gradientBuilder.Clear();
    }
}