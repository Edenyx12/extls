namespace extls.Core.Modules.Menu;

public class MMLayout : MMObj
{
    protected List<MMObj> objs = new();
    
    public AlignLayout align = AlignLayout.Horizontal;

    public MMLayout() : base(){}
    public MMLayout(MTTwo size, MTTwo pos) : base(size,pos){}
    public MMLayout(MTTwo size, MTTwo pos, AlignH alignh, AlignV alignv) : base(size,pos,alignh,alignv){}
    public MMLayout(AlignH alignh, AlignV alignv) : base(alignh,alignv){}

    public int Count() => objs.Count;

    public void Append(MMObj obj)
    {
        objs.Add(obj);
        Reposition();
    }
    
    public void Remove(MMObj obj)
    {
        if (!objs.Contains(obj)) return;
        objs.Remove(obj);
        Reposition();
    }

    private void Reposition()
    {
        
    }

    protected override void Draw()
    {
        int i = 0;

        foreach (MMObj obj in objs)
        {
            int iw = align is AlignLayout.Horizontal
                ? localAtlas.size.one / Count()
                : obj.size.one;
            int ih = align is AlignLayout.Vertical
                ? localAtlas.size.two / Count()
                : obj.size.two;
            
            int px = align is AlignLayout.Horizontal
                ? iw * i
                : obj.pos.one;
            int py = align is AlignLayout.Vertical
                ? ih * i
                : obj.pos.two;

            obj.size = new MTTwo(iw,ih);
            obj.pos  = new MTTwo(px,py);
            obj.Compose(ref localAtlas);

            i++;
        }
    }
}