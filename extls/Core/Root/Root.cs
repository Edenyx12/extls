using extls.Core.Decoration;
using extls.Core.Modules;

namespace extls.Core;

public partial class Root
{
    private string label = new string($"  $[Octavus]extls$[white] $[darkgray]v{Global.Version}\n");
    private string labelVersion = new string($"  $[Octavus]extls$[white] **$[platypus]v{Global.Version}**\n");
    private string help = new string(
        "  Help:\n" +
        "  * $[mint]`version`$[white], $[mint]`--version`$[white], $[mint]`-v`$[white] - colorize the version in the label.\n" +
        "  * $[mint]`help`$[white]   , $[mint]`--help`$[white]   , $[mint]`-h`$[white] - show this help.\n" +
        "  * $[mint]`modules`$[white], $[mint]`--modules`$[white], $[mint]`-m`$[white] - show a list of modules.\n" +
        "  * $[mint]`where`$[white]  , $[mint]`--where`$[white]  , $[mint]`-w`$[white] - add the bin path to the label.\n\n" +
        "  * $[festive]`--clear-cache`$[white] - clear the reflection module cache\n"
    );
    private string usage = new string(
        $"  $[navy]usage: **extls <MODULE> <MODULE-ARGS> <ARGS>**$[white]" +
        $"\n  $[darkgray]or:    extls root <args>\n"
    );

    public bool Dispatch(bool silent)
    {
        bool announce = false;
        bool label = false;

        if (Arguments.GetForce("v", "version"))
        {
            Version();
            label = true;
            announce = true;
        }
        else if (!silent) Label();

        if (Arguments.GetForce("w", "where"))
        {
            if (!label)
            {
                Label();
                label = true;
            }

            Where();
            announce = true;
        }

        if (!silent || announce)
            Markup.Line(new Gradient(Color.Lighter(Color.Octavus, 0.25f), Color.Darker(Color.Octavus, 0.67f)), preferred: 50);

        if (Arguments.GetForce("h", "help"))
        {
            if (!label)
            {
                Label();
                label = true;
            }

            Help();
            Markup.Line(new Gradient(Color.Lighter(Color.Octavus, 0.25f), Color.Darker(Color.Octavus, 0.67f)), preferred: 50);
            announce = true;
        }

        if (Arguments.GetForce("m", "modules"))
        {
            if (!label) Label();

            Modules();
            Markup.Line(new Gradient(Color.Lighter(Color.Octavus, 0.25f), Color.Darker(Color.Octavus, 0.67f)), preferred: 50);
            announce = true;
        }

        if (!silent || announce) Markup.Rich(usage);

        return announce;
    }

    public void Label() => Markup.Rich(label);
    public void Version() => Markup.Rich(labelVersion);
    public void Help() => Markup.Rich(help);
    public void Where()
    {
        Markup.Rich("  $[gray]bin path: ");

        string rawPath = AppDomain.CurrentDomain.BaseDirectory;
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string path = Markup.SafeBackslash(rawPath.Replace(userProfile, "~"));

        Markup.Rich($"**$[genesis]{path}**$[white]", true);
    }
    public void Modules()
    {
        string modules = $"  $[mint]Modules ({Global.Modules.Count}):$[white]\n";

        foreach (var key in Global.Modules)
        {
            Module? module = Global.GetModule(key.Key[0]);
            ModuleMeta? meta = Global.GetModuleMeta(module!.name);

            if (meta is null) continue;

            int dot = meta.TypeName.LastIndexOf('.') + 1;
            string moduleName = meta.TypeName[dot..];
            string aliases = "**$[nona]without aliases$[white]**";

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

            modules += $"  \\*  **$[octavus]{moduleName}$[white]** " +
                       $"$[darkgray]{module.version}$[white]: " +
                       $"{aliases}";
        }

        Markup.Rich(modules, true);
    }
}