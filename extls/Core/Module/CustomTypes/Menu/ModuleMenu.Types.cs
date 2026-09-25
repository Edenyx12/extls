using extls.Core.Decoration;

namespace extls.Core.Modules.Menu;

public enum MTTheme{Dark,Light}

public class MTAtlas{
    public MTCell[,] atlas;
    public MTTwo size;

    public MTAtlas()
    {
        this.size = new(1,1);
        this.atlas = new MTCell[
            this.size.two,
            this.size.one
        ];
    }
    public MTAtlas(MTTwo size)
    {
        this.size = size;
        this.atlas = new MTCell[
            this.size.two,
            this.size.one
        ];
    }

    public void Refresh()
    {
        this.atlas = new MTCell[
            this.size.two,
            this.size.one
        ];
        Clear();
    }

    public void Clear()
    {
        for (int y = 0; y < size.two; y++)
        {
            for (int x = 0; x < size.one; x++)
            {
                atlas[y,x] = new(' ');
            }
        }
    }

    public void PutIn(MTAtlas put, MTTwo pos)
    {
        if (
            pos.one >= this.size.one ||
            pos.two >= this.size.two ||
            pos.one + put.size.one <= 0 ||
            pos.two + put.size.two <= 0
        )
            return;

        int sx = Math.Max(0, pos.one);
        int sy = Math.Max(0, pos.two);

        int ex = Math.Min(this.size.one, pos.one + put.size.one);
        int ey = Math.Min(this.size.two, pos.two + put.size.two);

        for (int y = sy; y < ey; y++)
        {
            for (int x = sx; x < ex; x++)
            {
                int cy = y - pos.two;
                int cx = x - pos.one;

                atlas[y, x] = put.atlas[cy, cx];
            }
        }
    }
}
public record struct MTCell(char c, Color fg, Color bg){
    public MTCell() : this(' ', GetFgByTheme(), GetBgByTheme()){}
    public MTCell(char c) : this(c, GetFgByTheme(), GetBgByTheme()){}
    public MTCell(Color fg) : this(' ', fg, GetBgByTheme()){}
    public MTCell(Color fg, Color bg) : this(' ', fg, bg){}

    public static Color GetFgByTheme()
    {
        return ModuleMenu.theme switch
        {
            MTTheme.Light => new Color(0x00,0x00,0x00),
            MTTheme.Dark  => new Color(0xff,0xff,0xff),
            _ =>             new Color(0xff,0xff,0xff)
        };
    }
    public static Color GetBgByTheme()
    {
        return ModuleMenu.theme switch
        {
            MTTheme.Light => new Color(0xff,0xff,0xff),
            MTTheme.Dark  => new Color(0x00,0x00,0x00),
            _ =>             new Color(0x00,0x00,0x00)
        };
    }
}

public record struct MTTwo(int one = 1, int two = 1);
public enum AlignV{Top, Bottom, Center, Stretch, Absolute}
public enum AlignH{Left, Right, Center, Stretch, Absolute}
public enum AlignLayout{Horizontal, Vertical}