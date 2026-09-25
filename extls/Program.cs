using System.Collections.Immutable;
using extls.Core;
using extls.Core.Decoration;

namespace extls;

public class Program
{
    static void Main(string[] args)
    {
        var root = new Root();

        if (args.Length == 0)
        {
            root.Label();
            return;
        }

        Arguments.Initialize(args);
        bool announce = root.Dispatch(true);

        if (announce || Arguments.UsedGlobalArgs) return;

        string? moduleName = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (Markup.Match(args[i], 0, "--")) continue;
            else if (Markup.Match(args[i], 0, "-")) continue;

            moduleName = args[i].ToLower();
            break;
        }

        if (moduleName is null)
        {
            Utils.InvalidOperation();
            return;
        }

        List<string> cleanArgs = args.ToList();
        cleanArgs.Remove(moduleName);

        Arguments.Initialize(cleanArgs.ToArray());

        if (moduleName is "root")
        {
            root.Dispatch(true);
            return;
        }

        var module = Global.GetModule(moduleName);
        
        if (module != null)
        {
            module.Dispatch(cleanArgs.Count > 0 ? cleanArgs[0] : "");
            return;
        }
        
        if (!announce)
            Out.Warning($"Module not found: '{moduleName}'. Check modules with `extls root modules`.");
    }
}