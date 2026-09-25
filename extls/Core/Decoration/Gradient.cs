namespace extls.Core.Decoration;

public readonly struct Gradient
{
    public readonly Color start;
    public readonly Color end;

    public Gradient(Color start, Color end)
    {
        this.start = start;
        this.end = end;
    }

    public Color Pos(float t)
    {
        t = Math.Clamp(t, 0f, 1f);

        return new Color(
            (byte)(start.r + (end.r - start.r) * t),
            (byte)(start.g + (end.g - start.g) * t),
            (byte)(start.b + (end.b - start.b) * t)
        );
    }

    public string PaintString(string str)
    {
        string painted = string.Empty;

        for (int i = 0; i < str.Length; i++)
        {
            float t = i / (float)(str.Length - 1);
            Color color = Pos(t);

            painted += Color.ColorToConsoleFg(color) + str[i];
        }

        return painted;
    }
}