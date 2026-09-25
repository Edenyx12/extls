namespace extls.Core.Modules.Menu;

/// Lines:
/// ┼ ├ ┤ ┬ ┴ │ ─
/// ┌┐
/// └┘
/// ╭╮
/// ╰╯

public class MMObj
{
    private MTTwo _size;
    private MTTwo _pos;

    public MTTwo size
    {
        get => _size;
        set => _size = new MTTwo(
            Math.Clamp(value.one, 1, clampX ? Console.WindowWidth - 1 : 99999),
            Math.Clamp(value.two, 1, clampY ? Console.WindowHeight - 1 : 99999));
    }
    public MTTwo pos
    {
        get => _pos;
        set => _pos = new MTTwo(
            clampX ? Math.Clamp(value.one, 0, Console.WindowWidth - _size.one) : value.one,
            clampY ? Math.Clamp(value.two, 0, Console.WindowHeight - _size.two) : value.two);
    }

    public AlignH alignh = AlignH.Absolute;
    public AlignV alignv = AlignV.Absolute;

    public bool clampX = true;
    public bool clampY = true;

    protected MTAtlas localAtlas;

    private MTTwo dzx;
    private MTTwo dzy;

    public MMObj() : this(new MTTwo(2, 2), new MTTwo(0, 0))
    {
        this.localAtlas = new MTAtlas(new MTTwo(1,1));
    }

    public MMObj(MTTwo size, MTTwo pos)
    {
        this.size = size;
        this.pos = pos;
        this.localAtlas = new MTAtlas(this.size);
    }

    public MMObj(MTTwo size, MTTwo pos, AlignH alignh, AlignV alignv) : this(size, pos)
    {
        this.alignh = alignh;
        this.alignv = alignv;
    }

    public MMObj(AlignH alignh, AlignV alignv) : this(new MTTwo(2, 2), new MTTwo(0, 0))
    {
        this.alignh = alignh;
        this.alignv = alignv;
        this.localAtlas = new MTAtlas(this.size);
    }

    public void Compose(ref MTAtlas atlas)
    {
        CalcDz(new MTTwo(atlas.size.two, atlas.size.one));

        localAtlas.size = new MTTwo(
            dzx.two - dzx.one,
            dzy.two - dzy.one
        );
        localAtlas.Refresh();

        this.Draw();

        atlas.PutIn(localAtlas, new MTTwo(dzx.one, dzy.one));
    }

    protected virtual void Draw(){}

    private void CalcDz(MTTwo hw)
    {
        int px = alignh switch
        {
            AlignH.Right => hw.two - 1 - size.one,
            AlignH.Center => (hw.two / 2) - (size.one / 2),
            AlignH.Absolute => pos.one,
            _ or AlignH.Left or AlignH.Stretch => 0
        };
        int py = alignv switch
        {
            AlignV.Bottom => hw.one - 1 - size.two,
            AlignV.Center => (hw.one / 2) - (size.two / 2),
            AlignV.Absolute => pos.two,
            _ or AlignV.Top or AlignV.Stretch => 0
        };

        int lx = alignh switch
        {
            AlignH.Left => px + size.one,
            AlignH.Right or AlignH.Stretch => hw.two,
            _ or AlignH.Absolute => px + size.one 
        };
        int ly = alignv switch
        {
            AlignV.Top => py + size.two,
            AlignV.Bottom or AlignV.Stretch => hw.one,
            _ or AlignV.Absolute => py + size.two
        };

        dzx = new MTTwo(px, lx);
        dzy = new MTTwo(py, ly);
    }
}