using extls.Core.Decoration;

namespace extls.Core;

public abstract class Module
{
    public string name = "module";
    public string version = "0.0";

    public virtual void Label()
    {
        Markup.Rich($"\n  **$[catppuccin]{name}$[white]** $[darkgray]{version}$[white]", true);

        string methods = string.Empty;

        var meta = Global.GetModuleMeta(name);
        if (meta is null || meta.Methods is null || meta.Methods.Length is 0) 
        {
            methods = "  $[yellow]Without commands.$[white]\n";
            Markup.Line(new Gradient(Color.Octavus, Color.Darker(Color.Navy, 0.5f)));
            Markup.Rich(methods, true);
            return;
        }
        else methods = "  $[#82db70]Commands:$[white]\n";

        Markup.Line(new Gradient(Color.Octavus, Color.Darker(Color.Navy, 0.5f)));

        foreach (var method in meta.Methods)
        {
            methods += GetMethodItem(method);
        }

        Markup.Rich(methods, true);
    }

    public virtual void Help()
    {
        Markup.Rich($"\n  **$[catppuccin]{name}$[white]** $[darkgray]{version}$[white]", true);

        string help = string.Empty;

        var meta = Global.GetModuleMeta(name);
        if (meta is null || meta.Methods is null || meta.Methods.Length is 0) 
        {
            help = "  $[yellow]Without commands.$[white]\n";
            Markup.Line(new Gradient(Color.Octavus, Color.Darker(Color.Navy, 0.5f)));
            Markup.Rich(help, true);
            return;
        }

        Markup.Line(new Gradient(Color.Octavus, Color.Darker(Color.Navy, 0.5f)));

        foreach (var method in meta.Methods)
        {
            help += GetMethodItem(method);

            bool needDesc = method.Description is not "";
            if (!needDesc) continue;

            string desc = "    ";

            for (int i = 0; i < method.Description.Length; i++)
            {
                if (method.Description[i] is '\n')
                {
                    desc += "\n    ";
                    i++;

                    for (int j = i; j < method.Description.Length; j++)
                    {
                        if (method.Description[j] is not ' ')
                        {
                            i--;
                            break;
                        }
                        i++;
                    }

                    continue;
                }

                desc += method.Description[i];
            }

            help += desc + "\n\n";
        }

        Markup.Rich(help, true);
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

        if (arg is "-h" or "--help")
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

    private string GetMethodItem(MethodMeta meta)
    {
        string aliases = string.Empty;
        string methodName = string.Empty;

        if (meta.Aliases.Length == 1) aliases = meta.Aliases[0];
        else if (meta.Aliases.Length > 1)
        {
            for (int i = 0; i < meta.Aliases.Length; i++)
            {
                if (i == meta.Aliases.Length - 1)
                    aliases += meta.Aliases[i];
                else aliases += $"{meta.Aliases[i]}, ";
            }
        }

        methodName += char.ToUpper(meta.MethodName[0]);

        for (int i = 1; i < meta.MethodName.Length; i++)
        {
            if (char.IsUpper(meta.MethodName[i])) methodName += ' ';
            methodName += meta.MethodName[i];
        }

        return $"  • **$[mint]{methodName}$[white]**: {aliases}\n";
    }
}