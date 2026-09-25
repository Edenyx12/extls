using System.Text;

namespace extls.Core.Modules.Menu;

public class MMText : MMObj
{
    private string _text = string.Empty;
    private StringBuilder raw = new();
    private List<string> strokes = new();

    public string text
    {
        get => _text;
        set
        {
            _text = value;
            Reposition();
        }
    }

    public bool elide = false;

    public MMText() : base(){}
    public MMText(MTTwo size, MTTwo pos) : base(size,pos){}
    public MMText(MTTwo size, MTTwo pos, AlignH alignh, AlignV alignv) : base(size,pos,alignh,alignv){}
    public MMText(AlignH alignh, AlignV alignv) : base(alignh,alignv){}

    private void Reposition()
    {
        strokes.Clear();
        raw.Clear();

        if (elide)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == localAtlas.size.one - 3)
                {
                    raw.Append(text[i]);
                    raw.Append("...");

                    strokes.Add(raw.ToString());
                    raw.Clear();
                    return;
                }

                if (text[i] == '\n')
                {
                    if (i+1 >= text.Length && text[i+1] is ' ')
                    {
                        raw.Append(' ');
                        i++;
                        continue;
                    }

                    raw.Append(' ');
                    continue;
                }
            }
        }

        for (int i = 0; i < text.Length; i++)
        {
            if (i > localAtlas.size.one)
            {
                raw.Append(text[i]);
                strokes.Add(raw.ToString());
                raw.Clear();
                continue;
            }

            if (text[i] is '\n' && raw.Length == 0) continue;                
            else if (text[i] is '\n')
            {
                strokes.Add(raw.ToString());
                raw.Clear();
                continue;
            }

            raw.Append(text[i]);
        }

        if (raw.Length > 0) strokes.Add(raw.ToString());
    }

    protected override void Draw()
    {
        for (int y = 0; y < localAtlas.size.two; y++)
        {
            if (y > strokes.Count) return;

            for (int x = 0; x < localAtlas.size.one; x++)
            {
                if (x >= strokes[y].Length) continue;
                else if (strokes[y][x] is ' ') continue;

                localAtlas.atlas[y,x].c = strokes[y][x];
            }
        }
    }
}