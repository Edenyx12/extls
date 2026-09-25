using extls.Core;

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

        string moduleName = args[0].ToLower();;
        string[] cleanArgs = args[1..];

        Arguments.Initialize(cleanArgs);

        if (moduleName is "root")
        {
            root.Dispatch();
            return;
        }
        else if (moduleName is "-v" or "--version" or "version")
        {
            root.Version();
            return;
        }
        else if (moduleName is "-h" or "--help" or "help")
        {
            root.Help();
            return;
        }

        var module = Global.GetModule(moduleName);
        
        if (module != null)
        {
            module.Dispatch(cleanArgs.Length > 0 ? cleanArgs[0] : "");
            return;
        }
        
        Out.Warning($"Module not found: '{moduleName}'. Check modules with `extls root modules`.");
    }
}