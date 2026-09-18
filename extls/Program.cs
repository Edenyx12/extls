using extls.Core;
using extls.Core.Decoration;

namespace extls;

public class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Markup.Rich($"$[#ead9fa]extls: $[cyan]{Root.Version}");
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
            if (arg == "--clear-cache")
            {
                if (File.Exists(Path.Combine(Root.RootPath, "modules.json")))
                    File.Delete(Path.Combine(Root.RootPath, "modules.json"));
                
                Markup.Rich($"**$[blue]modules.json$[white]** deleted from $[yellow]{Markup.FixBackslash(Root.RootPath)}");
                return;
            }
            
            cleanArgs.Add(args[i]);
        }

        if (cleanArgs.Count == 0) return;

        switch (cleanArgs[0])
        {
            case "version" or "--version" or "-v":
                Markup.Rich($"$[#ead9fa]extls: $[cyan]{Root.Version}");
                return;
            case "help" or "-h" or "--help":
                Markup.Rich($"$[#ead9fa]extls: $[cyan]{Root.Version}:\n$[darkgray]" +
                            $"  --version / version - Check extls version.\n" +
                            $"  -v - Alias on --version.\n" +
                            $"  --help / help - Show this help text.\n" +
                            $"  -h - Alias on --help.\n" +
                            $"  --verbose - Enable verbose logging.\n" +
                            $"  --clear-cache - Clear reflection cache.\n" +
                            $"  modules - Show all modules.\n" +
                            $"  where - Show path on this process.\n" +
                            $"\n$[green]extls <MODULE> <MODULE-ARGS> <ARGS>\n");
                return;
            case "modules":
                Print.Line("extls modules:");
                foreach (var key in Root.Modules)
                {
                    var mdl = Root.GetModule(key.Key[0]);
                    if (mdl != null) mdl.Version();
                }
                return;
            case "where": Print.Line(AppDomain.CurrentDomain.BaseDirectory, ConsoleColor.Green); return;
        }

        var module = Root.GetModule(cleanArgs[0]);
        
        if (module != null)
        {
            module.Dispatch(Utils.RemoveZeroCommand(cleanArgs.ToArray()));
            return;
        }
        
        Print.Warning($"Module not found: '{cleanArgs[0]}'. Check modules with `extls modules`.`");
    }
}