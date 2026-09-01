using extls.Core;
using extls.Tools;
using System;
using System.Reflection;

namespace extls;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Print.Line($"extls {Root.Version}");
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
                Print.Line($"extls {Root.Version}");
                return;
            case "help" or "-h" or "--help":
                Assembly assembly = Assembly.GetExecutingAssembly();

                var derivedTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(extls.Core.Module)));

                foreach (var type in derivedTypes)
                {
                    try
                    {
                        object instance = Activator.CreateInstance(type)!;

                        MethodInfo method = type.GetMethod("Version")!;

                        if (method != null) method.Invoke(instance, null);
                    }
                    catch { }
                }

                return;
            case "where": Print.Line(AppDomain.CurrentDomain.BaseDirectory, ConsoleColor.Green); return;
            default: break;
        }

        extls.Core.Module module = GetModule(args[0])!;
        if (module != null) module.Dispatch(Utils.RemoveZeroCommand(args));
        else  Print.Warning($"Module not found: '{args[0]}'.");
    }

    static extls.Core.Module? GetModule(string arg)
    {
        return arg switch
        {
            "ai" => new AI(),
            "ai.apikeys" => new ApiKeys(),
            "calc" or "calculate" => new Calculator(),
            "dir" => new Dir(),
            "alias" => new Alias(),
            _ => null
        };
    }
}