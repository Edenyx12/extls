using System.Text;
using extls.Core.Decoration;

namespace extls.Core;

public static class Arguments
{
    public static bool UsedGlobalArgs = false;
    public static Arg[]? argv;
    public static Arg[]? arglong;
    public static Arg[]? argshorts;
    public static Arg[]? argraw;

    public static void Initialize(string[]? args)
    {
        if (args is null) return;
        if (args.Length == 0) return;
        argv = null;
        arglong = null;
        argshorts = null;
        argraw = null;

        string[] clean = ParseGlobal(args);
        Parse(clean);
    }

    public static bool Get(params string[] args)
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
    
    public static bool GetForce(params string[] args)
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

    public static string? GetRight(params string[] args)
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

    public static int GetIndex(params string[] args)
    {
        if (argv is null) return -1;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Length == 1)
                foreach (Arg arg1 in argv)
                    if (arg1.Contains(args[i][0])) return arg1.index;
            else 
                foreach (Arg arg2 in argv)
                    if (arg2.Contains(args[i])) return arg2.index;
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

    public static string? GetPath()
    {
        if (argraw is null) return null;

        string? path = null;

        foreach (Arg arg in argraw)
        {
            path = ParsePath(arg.str);
            if (path is null) continue;

            return path;
        }

        return null;
    }

    public static string[]? GetPaths()
    {
        if (argraw is null) return null;

        List<string> paths = new();

        foreach (Arg arg in argraw)
        {
            string? path = ParsePath(arg.str);
            if (path is null) continue;

            paths.Add(path);
        }

        if (paths.Count > 0) return paths.ToArray();
        else return null;
    }

    public static string? ParsePath(string? path)
    {
        if (path is null || path.Length is 0) return null;

        string? parsed = path[0] switch {
            '.' => path.Length > 2
                ? Path.Combine(Directory.GetCurrentDirectory(), new string(path?[2..]))
                : Directory.GetCurrentDirectory(),
            '~' => path.Length > 2
                ? Path.Combine(Global.HomePath, new string(path?[2..]))
                : Global.HomePath,
            _ => path
        };

        if (!Directory.Exists(parsed)) parsed = null;
        return parsed;
    }

    public static bool ParsePath(string? path, out string? parsed)
    {
        if (path is null || path.Length is 0)
        {
            parsed = null;
            return false;
        }

        parsed = path[0] switch {
            '.' => path.Length > 2
                ? Path.Combine(Directory.GetCurrentDirectory(), new string(path?[2..]))
                : Directory.GetCurrentDirectory(),
            '~' => path.Length > 2
                ? Path.Combine(Global.HomePath, new string(path?[2..]))
                : Global.HomePath,
            _ => path
        };

        if (!Directory.Exists(parsed))
        {
            parsed = null;
            return false;
        }

        return true;
    }

    private static string[] ParseGlobal(string[] args)
    {
        List<string> clean = new();

        int count = 0;

        for (int arg = 0; arg < args.Length; arg++)
        {
            bool triggered = false;

            switch (args[arg])
            {
                case "--verbose":
                    triggered = true;
                    Global.Verbose = true;
                    break;
                case "--clear-cache":
                    triggered = true;
                    if (File.Exists(Path.Combine(Global.RootPath, "modules.json")))
                        File.Delete(Path.Combine(Global.RootPath, "modules.json"));
                
                    Markup.Rich($"[root] **$[octavus]modules.json$[white]** deleted from " +
                                $"$[yellow]{Markup.SafeBackslash(Global.RootPath)}$[white]", true);
                    count++;
                    break;
            }

            UsedGlobalArgs = triggered;

            if (triggered) continue;

            clean.Add(args[arg]);
        }

        if (count > 0) Console.WriteLine();

        return clean.ToArray();
    }

    private static void Parse(string[] args)
    {
        List<Arg> argv      = new();
        List<Arg> arglong   = new();
        List<Arg> argshorts = new();
        List<Arg> argraw    = new();

        for (int arg = 0; arg < args.Length; arg++)
        {
            if (Markup.Match(args[arg], 0, "--"))
            {
                string slice = args[arg].Substring(2).ToLower();
                var argl = new Arg(slice, arg, ArgType.Long);
                argv.Add(argl);
                arglong.Add(argl);
                continue;
            }
            else if (Markup.Match(args[arg], 0, "-"))
            {
                string slice = args[arg].Substring(1).ToLower();
                var argsh = new Arg(slice, arg, ArgType.Shorts);
                argv.Add(argsh);
                argshorts.Add(argsh);
                continue;
            }

            var argr = new Arg(args[arg], arg, ArgType.Raw);
            argv.Add(argr);
            argraw.Add(argr);
        }

        Arguments.argv      = argv.ToArray();
        Arguments.arglong   = arglong.ToArray();;
        Arguments.argshorts = argshorts.ToArray();;
        Arguments.argraw    = argraw.ToArray();;
    }
}