using extls.Core.Decoration;
using extls.Core.Modules;

namespace extls.Core;

public partial class Root
{
    private string label = new string(
        $"\n  $[Octavus]extls$[white] $[darkgray]v{Global.Version}" +
        $"\n  $[navy]usage: **extls <MODULE> <MODULE-ARGS> <ARGS>**$[white]" +
        $"\n  $[darkgray]or:    extls root <args>\n"
    );
    private string labelVersion = new string(
        $"\n  $[Octavus]extls$[white] **$[platypus]v{Global.Version}**" +
        $"\n  $[navy]usage: **extls <MODULE> <MODULE-ARGS> <ARGS>**$[white]" +
        $"\n  $[darkgray]or:    **extls root <args>**$[white]\n"
    );
    private string help = new string(
        $"  Help:\n"
    );

    public void Dispatch()
    {
        if (Arguments.Get("v", "version")) Markup.Rich(labelVersion);
        else Version();

        if (Arguments.Get("where")) Where();

        if (Arguments.Get("h", "help"))
        {
            Out.Line($"  {new string('─', 40)}");
            Help();
        }

        if (Arguments.Get("modules"))
        {
            Out.Line($"  {new string('─', 40)}");
            Modules();
        }
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
        Out.Line("  modules:");

        foreach (var key in Global.Modules)
        {
            var module = Global.GetModule(key.Key[0]);
            if (module != null) module.Version();
        }
    }
}