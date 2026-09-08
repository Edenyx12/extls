namespace extls.Core.Decoration;

public readonly struct Color
{
    public int r { get; }
    public int g { get; }
    public int b { get; }

    public Color(int r, int g, int b)
    {
        this.r = r;
        this.g = g;
        this.b = b;
    }
    
    public override string ToString() => $"[{r}, {g}, {b}]";
    
    public static Color HEXToColor(string HEX)
    {
        string hex = HEX.Replace("#", "");
        int r = (HEXCharToInt(hex[0]) * 16) + HEXCharToInt(hex[1]);
        int g = (HEXCharToInt(hex[2]) * 16) + HEXCharToInt(hex[3]);
        int b = (HEXCharToInt(hex[4]) * 16) + HEXCharToInt(hex[5]);

        return new Color(r,g,b);
    }

    public static byte HEXCharToInt(char c)
        => c switch {
            '0' => 0,
            '1' => 1,
            '2' => 2,
            '3' => 3,
            '4' => 4,
            '5' => 5,
            '6' => 6,
            '7' => 7,
            '8' => 8,
            '9' => 9,
            'A' or 'a' => 10,
            'B' or 'b' => 11,
            'C' or 'c' => 12,
            'D' or 'd' => 13,
            'E' or 'e' => 14,
            'F' or 'f' => 15,
            _ => throw new Exception("Invalid HEX char")
        };
}