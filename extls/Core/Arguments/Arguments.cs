using System.Globalization;
using System.Text;
using extls.Core.Decoration;

namespace extls.Core;

public static class Arguments
{
    public static bool UsedGlobalArgs = false;
    public static string? firstRaw;
    public static List<Arg>? argv;
    public static List<Arg>? arglong;
    public static List<Arg>? argshorts;
    public static List<Arg>? argraw;

    private static HashSet<string> globalArgs = new();
    private static HashSet<string> addedArgs = new();

    public static void Initialize(string[]? args)
    {
        if (args is null) return;
        if (args.Length == 0) return;

        firstRaw = null;
        argv = null;
        arglong = null;
        argshorts = null;
        argraw = null;
        addedArgs.Clear();

        List<string> clean = ParseGlobal(args);
        Parse(clean);
    }

    public static bool Get(params ReadOnlySpan<string> args)
    {
        if (argv is null) return false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Length == 1 && argshorts is not null)
            {
                foreach (Arg arg in argshorts)
                    if (arg.Contains(args[i][0])) return true;

                continue;
            }

            if (arglong is not null)
                foreach (Arg arg in arglong)
                    if (arg.Contains(args[i])) return true;
        }

        return false;
    }
    
    public static bool GetForce(params ReadOnlySpan<string> args)
    {
        if (argv is null) return false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Length == 1)
            {
                foreach (Arg arg in argv)
                    if (arg.Contains(args[i][0])) return true;

                continue;
            }

            foreach (Arg arg in argv)
                if (arg.Contains(args[i])) return true;

        }
        return false;
    }

    public static string? GetRight(params ReadOnlySpan<string> args)
    {
        if (argv is null) return null;
        if (argraw is null) return null;

        int index = GetIndex(args);
        if (index is -1) return null;
        else index += 1;

        foreach (Arg arg in argraw)
        {
            if (index == arg.index) return arg.str;
        }

        return null;
    }

    public static int GetRightInt(ReadOnlySpan<string> args, int @default = -1)
    {
        string? right = GetRight(args);
        if (right is null) return @default;

        if (int.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)) return value;
        else return @default;
    }

    public static float GetRightFloat(ReadOnlySpan<string> args, float @default = -1f)
    {
        string? right = GetRight(args);
        if (right is null) return @default;

        if (float.TryParse(right, NumberStyles.Float, CultureInfo.InvariantCulture, out float value)) return value;
        else return @default;
    }

    public static int GetRightSwitch(ReadOnlySpan<string> args, ReadOnlySpan<string> items, int @default = -1)
    {
        string? right = GetRight(args);
        if (right is null) return @default;

        for (int i = 0; i < items.Length; i++)
            if (right == items[i]) return i;

        return @default;
    }

    public static int GetIndex(params ReadOnlySpan<string> args)
    {
        if (argv is null) return -1;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Length == 1)
            {
                foreach (Arg arg1 in argv)
                    if (arg1.Contains(args[i][0])) return arg1.index;
            }
            else
            {
                foreach (Arg arg2 in argv)
                    if (arg2.Contains(args[i]))return arg2.index;
            }
        }
        return -1;
    }

    public static string? GetRaw(int index)
    {
        if (argraw is null) return null;

        int rawIndex = 0;

        foreach (Arg arg in argraw)
        {
            if (rawIndex == index) return arg.str;
            rawIndex++;
        }

        return null;
    }

    public static string? GetPath(PathType type = PathType.Both)
    {
        if (argraw is null) return null;

        foreach (Arg arg in argraw)
        {
            string? path = ParsePath(arg.str, type);
            if (path is not null) return path;
        }

        return null;
    }

    public static string[]? GetPaths(PathType type = PathType.Both)
    {
        if (argraw is null) return null;

        List<string> paths = new();

        foreach (Arg arg in argraw)
        {
            string? path = ParsePath(arg.str, type);
            if (path is not null) paths.Add(path);
        }

        return paths.Count > 0 ? paths.ToArray() : null;
    }

    public static string? ParsePath(string? path, PathType type = PathType.Both)
        => ParsePath(path, out string? parsed, type) ? parsed : null;

    public static bool ParsePath(string? path, out string? parsed, PathType type = PathType.Both)
    {
        if (path is null || path.Length is 0)
        {
            parsed = null;
            return false;
        }

        switch (path![0])
        {
            case '.':
                if (Markup.Match(path, 0, "./") || Markup.Match(path, 0, @".\"))
                {
                    var slice = Markup.Slice(path, 2, path.Length);
                    parsed = Path.Combine(Directory.GetCurrentDirectory(), slice);
                }
                else if (Markup.Match(path, 0, "..")) parsed = Path.GetFullPath(path, Directory.GetCurrentDirectory());
                else
                {
                    if (path.Length is 1)
                        parsed = Directory.GetCurrentDirectory(); // ex: . (current)
                    else if (path.Length > 1)
                        parsed = Path.Combine(Directory.GetCurrentDirectory(), Markup.Slice(path, 1, path.Length)); // ex: .config from ~
                    else parsed = path;
                }
                break;
            case '~':
                if (Markup.Match(path, 0, "~/") || Markup.Match(path, 0, @"~\"))
                {
                    var slice = Markup.Slice(path, 2, path.Length);
                    parsed = Path.Combine(Global.HomePath, slice);
                }
                else
                {
                    if (path.Length is 1)
                        parsed = Global.HomePath;
                    else if (path.Length > 1)
                        parsed = Path.Combine(Global.RootPath, Markup.Slice(path, 1, path.Length));
                    else parsed = path;
                }
                break;
            default:
                parsed = path;
                break;
        }

        switch (type)
        {
            case PathType.File:
                if (!File.Exists(parsed))
                {
                    parsed = null;
                    return false;
                }
                break;
            case PathType.Folder:
                if (!Directory.Exists(parsed))
                {
                    parsed = null;
                    return false;
                }
                break;
            case PathType.Both:
                if (Directory.Exists(parsed))return true;
                else if (File.Exists(parsed)) return true;
                else
                {
                    parsed = null;
                    return false;
                }
        }

        return true;
    }

    public static void RemoveArg(string arg)
    {
        if (argv is null) return;
        RemoveItem(argv, arg);

        if (arglong is not null)   RemoveItem(arglong, arg);
        if (argshorts is not null) RemoveItem(argshorts, arg);
        if (argraw is not null)    RemoveItem(argraw, arg);

        void RemoveItem(List<Arg> list, string str)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].str == str)
                {
                    list.Remove(list[i]);
                    break;
                }
            }
        }
    }

    private static List<string> ParseGlobal(ReadOnlySpan<string> args)
    {
        List<string> clean = new();

        int count = 0;

        for (int arg = 0; arg < args.Length; arg++)
        {
            bool triggered = false;

            switch (args[arg])
            {
                case "--verbose":     if (!globalArgs.Add(args[arg])) break;
                    triggered = true;
                    Global.Verbose = true;
                    break;
                case "--clear-cache": if (!globalArgs.Add(args[arg])) break;
                    triggered = true;
                    if (File.Exists(Path.Combine(Global.RootPath, "modules.json")))
                        File.Delete(Path.Combine(Global.RootPath, "modules.json"));
                
                    Markup.Rich($"[root] **$[octavus]modules.json$[white]** deleted from " +
                                $"$[yellow]{Markup.SafeBackslash(Global.RootPath)}$[white]", true);
                    count++;
                    break;
            }

            if (triggered)
            {
                UsedGlobalArgs = true;
                continue;
            }

            clean.Add(args[arg]);
        }

        if (count > 0) Console.WriteLine();

        return clean;
    }

    private static void Parse(List<string> args)
    {
        List<Arg> argv      = new();
        List<Arg> arglong   = new();
        List<Arg> argshorts = new();
        List<Arg> argraw    = new();

        for (int arg = 0; arg < args.Count; arg++)
        {
            if (Markup.Match(args[arg], 0, "--"))
            {
                string slice = args[arg].Substring(2).ToLower();
                if (!addedArgs.Add(slice)) continue;
                var argl = new Arg(slice, arg, ArgType.Long);
                argv.Add(argl);
                arglong.Add(argl);
                continue;
            }
            else if (Markup.Match(args[arg], 0, "-"))
            {
                string slice = args[arg].Substring(1).ToLower();
                if (!addedArgs.Add(slice)) continue;
                var argsh = new Arg(slice, arg, ArgType.Shorts);
                argv.Add(argsh);
                argshorts.Add(argsh);
                continue;
            }

            if (!addedArgs.Add(args[arg])) continue;
            var argr = new Arg(args[arg], arg, ArgType.Raw);
            argv.Add(argr);
            argraw.Add(argr);
        }

        Arguments.argv      = argv;
        Arguments.arglong   = arglong;
        Arguments.argshorts = argshorts;
        Arguments.argraw    = argraw;

        if (argraw.Count > 0) firstRaw = argraw[0].str;
    }
}