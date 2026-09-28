using extls.Core;
using extls.Core.Decoration;
using extls.Core.Modules;
using extls.Core.Modules.Menu;

namespace extls;


public class Program
{
    static void Main(string[] args)
    {
        var root = new Root();

        if (args.Length == 0)
        {
            root.Dispatch(false);
            return;
        }

        Arguments.Initialize(args);

        string? moduleName = Arguments.firstRaw;

        if (moduleName is null)
        {
            root.Dispatch(false);
            return;
        }

        Module? module = Global.GetModule(moduleName);

        if (module != null)
        {
            Arguments.RemoveArg(moduleName);

            string arg = string.Empty;
            if (Arguments.argv is not null && Arguments.argv.Count > 0) arg = Arguments.argv[0].str;

            module.Dispatch(arg);

            return;
        }
        else if (moduleName is "root")
        {
            bool announce = root.Dispatch(true);
            if (!announce)
                Out.Warning($"Root: There are no arguments.");

            return;
        }
        
        Out.Warning($"Module not found: '{moduleName}'. Check modules with `extls root modules`.");
    }
}