using extls.Core;
using extls.Tools;
using System;

internal class Program
{
    public static string version = "0.2.32-alpha";

    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Print.Line($"extls {version}");
            return;
        }

        for (int i = 0; i < args.Length; i++)
        {
            args[i] = args[i].ToLower();

            if (args[i] == "--verbose") Print.verbose = true;
        }

        switch (args[0])
        {
            case "version" or "--version" or "-v":
                Print.Line($"extls {version}");
                return;
            case "where": Print.Line(AppDomain.CurrentDomain.BaseDirectory, ConsoleColor.Green); return;
            default: break;
        }

        Module module = GetModule(args[0])!;
        if (module != null) module.Dispatch(Utils.RemoveZeroCommand(args));
        else  Print.Warning($"Module not found: '{args[0]}'.");
    }

    static Module? GetModule(string arg)
    {
        return arg switch
        {
            "ai" => new AI(),
            "ai>apikeys" => new ApiKeys(),
            "calc" or "calculate" => new Calculator(),
            "dir" => new Dir(),
            _ => null
        };
    }
}