using System.Diagnostics.CodeAnalysis;

namespace extls.Core.Decoration;

public readonly struct Color
{
    public byte r { get; }
    public byte g { get; }
    public byte b { get; }

    public readonly static Color Nona = new Color(255, 74, 117);        // #ff4a75
    public readonly static Color NonaLux = new Color(233, 30, 99);      // #e91e63
    public readonly static Color Octavus = new Color(88, 166, 255);     // #58a6ff
    public readonly static Color Septima = new Color(163, 112, 247);    // #a370f7
    public readonly static Color Sextus = new Color(255, 123, 114);     // #ff7b72
    public readonly static Color Festive = new Color(218, 54, 51);      // #da3633
    public readonly static Color Genesis = new Color(225, 177, 44);     // #e1b12c
    public readonly static Color Catppuccin = new Color(205, 214, 244); // #cdd6f4
    public readonly static Color Mint = new Color(144, 224, 144);       // #90e090
    public readonly static Color PurpleRain = new Color(155, 89, 182);  // #9b59b6
    public readonly static Color Platypus = new Color(245, 158, 11);    // #f59e0b
    public readonly static Color Barbie = new Color(255, 105, 180);     // #ff69b4
    public readonly static Color Navy = new Color(65, 114, 159);        // #41729f
    public readonly static Color Cyanish = new Color(82, 183, 136);     // #52b788
    public readonly static Color SevenUp = new Color(34, 197, 94);      // #22c55e

    public Color(byte r, byte g, byte b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }

    public Color(string hex)
    {
        Color color = HEXToColor(hex);
        this.r = color.r;
        this.g = color.g;
        this.b = color.b;
    }
    
    public override string ToString() => $"[{r}, {g}, {b}]";
    public bool Equals(Color c) => r == c.r && g == c.g && b == c.b;
        
    public static Color HEXToColor(ReadOnlySpan<char> HEX)
    {
        ReadOnlySpan<char> hex = HEX;
        if (!hex.IsEmpty && hex[0] == '#')
            hex = hex.Slice(1);

        if (hex.Length != 6)
            throw new ArgumentException("HEX must be 6 chars (RRGGBB)", nameof(HEX));

        int r = (HEXCharToInt(hex[0]) << 4) | HEXCharToInt(hex[1]);
        int g = (HEXCharToInt(hex[2]) << 4) | HEXCharToInt(hex[3]);
        int b = (HEXCharToInt(hex[4]) << 4) | HEXCharToInt(hex[5]);

        return new Color((byte)r, (byte)g, (byte)b);
    }

    public static byte HEXCharToInt(char c)
    {
        int val = c - '0';
        if ((uint)val <= 9) return (byte)val;

        val = (c & ~0x20) - 'A';
        if ((uint)val <= 5) return (byte)(val + 10);

        throw new ArgumentException($"Invalid HEX char: '{c}'", nameof(c));
    }

    public static string ColorToConsoleFg(Color c) => $"\x1b[38;2;{c.r};{c.g};{c.b}m";
    public static string ColorToConsoleFg(ReadOnlySpan<char> HEX)
    {
        Color c = HEXToColor(HEX);
        return $"\x1b[38;2;{c.r};{c.g};{c.b}m";
    }
    public static string ColorToConsoleBg(Color c) => $"\x1b[48;2;{c.r};{c.g};{c.b}m";
    public static string ColorToConsoleBg(ReadOnlySpan<char> HEX)
    {
        Color c = HEXToColor(HEX);
        return $"\x1b[48;2;{c.r};{c.g};{c.b}m";
    }

    public static Color Lighter(Color color, float light)
    {
        light = Math.Clamp(light, 0, 99999);

        byte r = (byte)Math.Clamp(color.r + (255 - color.r) * light, 0, 255);
        byte g = (byte)Math.Clamp(color.g + (255 - color.g) * light, 0, 255);
        byte b = (byte)Math.Clamp(color.b + (255 - color.b) * light, 0, 255);

        return new Color(r, g, b);
    }

    public static Color Darker(Color color, float dark)
    {
        dark = Math.Clamp(dark, 0, 99999);

        byte r = (byte)Math.Clamp(color.r * (1 - dark), 0, 255);
        byte g = (byte)Math.Clamp(color.g * (1 - dark), 0, 255);
        byte b = (byte)Math.Clamp(color.b * (1 - dark), 0, 255);

        return new Color(r, g, b);
    }
}