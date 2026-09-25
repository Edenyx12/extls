namespace extls.Core;

public enum ArgType{Shorts, Long, Raw}

public class Arg
{
    public ArgType type = ArgType.Raw;
    public char[] chars;
    public string str;
    public int index;

    public Arg(string args, int index, ArgType type = ArgType.Raw)
    {
        chars = args.ToCharArray();
        str = args;
        this.index = index;
        this.type = type;
    }

    public bool Contains(string arg) => str == arg;
    public bool Contains(char c)
    {
        if (type != ArgType.Shorts) return false;

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == c) return true;
        }

        return false;
    }
}