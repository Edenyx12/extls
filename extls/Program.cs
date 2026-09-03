using extls.Core;
using extls.Tools;
using System.Reflection;

namespace extls;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Markup.Rich($"extls: [cyan]{Root.Version}", null!);
            return;
        }

        List<string> cleanArgs = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i].ToLower();

            if (arg == "--verbose")
            {
                Print.verbose = true;
                continue;
            }
            if (arg == "-clrm")
            {
                if (File.Exists(Path.Combine(Root.RootPath, "modules.json")))
                    File.Delete(Path.Combine(Root.RootPath, "modules.json"));
                
                if (!File.Exists(Path.Combine(Root.RootPath, "modules.json")))
                    Markup.Rich($"modules.json deleted in [yellow]{Root.RootPath}", null!);
                return;
            }
            
            cleanArgs.Add(args[i]);
        }

        if (cleanArgs.Count == 0) return;

        switch (cleanArgs[0])
        {
            case "version" or "--version" or "-v":
                Print.Line($"extls {Root.Version}");
                return;
            case "help" or "-h" or "--help":
                Markup.Rich($"extls {Root.Version}:\n[darkgray]" +
                            $"  --version / version - Check extls version.\n" +
                            $"  -v - Alias on --version.\n" +
                            $"  --help / help - Show this help text.\n" +
                            $"  -h - Alias on --help.\n" +
                            $"  --verbose - Enable verbose logging.\n" +
                            $"  modules - Show all modules.\n" +
                            $"  where - Show path on this process.\n" +
                            $"\n[green]extls <MODULE> <MODULE-ARGS> <ARGS>\n", null!);
                return;
            case "modules":
                Print.Line("extls modules:");
                foreach (var key in Root.Modules.Keys)
                {
                    var mdl = Root.GetModule(key[0]);
                    if (mdl != null) mdl.Version();
                }
                return;
            case "where": Print.Line(AppDomain.CurrentDomain.BaseDirectory, ConsoleColor.Green); return;
        }

        var module = Root.GetModule(cleanArgs[0]);
        
        if (module != null)
        {
            module.Dispatch(Utils.RemoveZeroCommand(args));
            return;
        }
        
        Print.Warning($"Module not found: '{cleanArgs[0]}'. Check modules with `extls modules`.`");
    }
}