using System.Text;
using extls.Core;
using extls.Core.Decoration;

namespace extls.Tools;

[ModuleName("parse", "parser", "prs")]
public partial class Parser : Module
{
    public Parser()
    {
        name = "parser";
        version = "0.0.1a";
    }

    [MethodName(
        description: "Replace all occurrences.",
        aliases: ["replace", "rplc", "rc"]
    )]
    public void Replace()
    {
        string? from = Arguments.GetRaw(0);
        string? to   = Arguments.GetRaw(1);
        string? path = Arguments.GetPath(PathType.File);

        if (path is null)
        {
            Out.Warning("Path are missing.");
            return;
        }
        else if (from is null || to is null)
        {
            Out.Warning("Arguments are missing");
            return;
        }

        string file = File.ReadAllText(path);
        StringBuilder b = new(file.Length);

        for (int i = 0; i < file.Length; i++)
        {
            if (Markup.Match(file, i, from))
            {
                b.Append(to);
                i += from.Length-1;
                continue;
            }

            b.Append(file[i]);
        }

        File.WriteAllText(path, b.ToString());
    }
}