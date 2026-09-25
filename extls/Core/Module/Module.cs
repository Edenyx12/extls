using extls.Core.Decoration;

namespace extls.Core;

public abstract class Module
{
    public string name = "module";
    public string version = "0.0";

    public virtual void Label()
    {
        Markup.Rich($"\n  **$[catppuccin]{name}$[white]** $[darkgray]{version}$[white]", true);
        Markup.Line(new Gradient(Color.Octavus, Color.Darker(Color.Navy, 0.5f)));

        string methods = string.Empty;

        var meta = Global.GetModuleMeta(name);
        if (meta is null) 
        {
            methods = "  $[yellow]Without commands.$[white]\n";
            Markup.Rich(methods, true);
            return;
        }
        else methods = "  $[#82db70]Commands:$[white]\n";

        foreach (var method in meta.Methods)
        {
            string aliases = string.Empty;
            string methodName = string.Empty;

            if (method.Aliases.Length == 1) aliases = method.Aliases[0];
            else if (method.Aliases.Length > 1)
            {
                for (int i = 0; i < method.Aliases.Length; i++)
                {
                    if (i == method.Aliases.Length - 1)
                        aliases += method.Aliases[i];
                    else aliases += $"{method.Aliases[i]}, ";
                }
            }

            methodName += char.ToUpper(method.MethodName[0]);

            for (int i = 1; i < method.MethodName.Length; i++)
            {
                if (char.IsUpper(method.MethodName[i])) methodName += ' ';
                methodName += method.MethodName[i];
            }

            methods += $"  \\*  **$[mint]{method.MethodName}$[white]**: {aliases}\n";
        }

        Markup.Rich(methods, true);
    }

    public virtual void Help()
    {
        
    }
    public virtual void Version()
        => Markup.Rich($"\nModule '**$[catppuccin]{name}$[white]**': **$[sextus]{version}$[white]** version.");

    public virtual bool Dispatch(string arg)
    {
        if (arg == "")
        {
            Label();
            return true;
        }

        if (arg is "-v" or "--version")
        {
            Version();
            return true;
        }

        if (arg is "help" or "-h" or "--help")
        {
            Help();
            return true;
        }

        var meta = Global.GetModuleMeta(name);
        if (meta is null)
        {
            Utils.InvalidOperation();
            return false;
        }

        return Global.ExecuteModule(this, meta, arg);
    }
}