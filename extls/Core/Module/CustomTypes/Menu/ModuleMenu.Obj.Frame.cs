using extls.Core.Decoration;

namespace extls.Core.Modules.Menu;

/// Lines:
/// ┼ ├ ┤ ┬ ┴ │ ─
/// ┌┐
/// └┘
/// ╭╮
/// ╰╯

public class MMFrame : MMObj
{
    public Color foreground = MTCell.GetFgByTheme();
    public Color background = MTCell.GetBgByTheme();

    public MMFrame() : base(){}
    public MMFrame(MTTwo size, MTTwo pos) : base(size,pos){}
    public MMFrame(MTTwo size, MTTwo pos, AlignH alignh, AlignV alignv) : base(size,pos,alignh,alignv){}
    public MMFrame(AlignH alignh, AlignV alignv) : base(alignh,alignv){}
    
    protected override void Draw()
    {
        for (int y = 0; y < localAtlas.size.two; y++)
        {
            for (int x = 0; x < localAtlas.size.one; x++)
            {
                char p = ' ';

                bool left = x == 0;
                bool right = x == localAtlas.size.one - 1;
                bool top = y == 0;
                bool bottom = y == localAtlas.size.two - 1;

                if (top && left)          p = '╭';
                else if (top && right)    p = '╮';
                else if (bottom && left)  p = '╰';
                else if (bottom && right) p = '╯';
                else if (top || bottom)   p = '─';
                else if (left || right)   p = '│';

                localAtlas.atlas[y, x] = p == ' ' ? new(p) : new(p,foreground,background);
            }
        }
    }
}