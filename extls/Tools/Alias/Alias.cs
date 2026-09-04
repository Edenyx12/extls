using extls.Core;
using extls.Core.Modules;
using System.Diagnostics;

namespace extls.Tools;

public record AliasItem(string name, string alias);

[ModuleName("alias")]
public partial class Alias : ModuleMenu
{
    public Alias()
    {
        name = "alias";
        version = "0.2a";
        commands = null!;
    }

    public override bool Dispatch(string[] args)
    {
        if (args.Length > 0 && !(args[0] is "--help" or "-h" or "help" or "-v" or "--version"))
        {
            if (Root.Platform is Platform.Linux)
            {
                Print.Warning("Linux is not supported on this version.");
                return false;
            }

            ExecuteAlias(args);
            return true;
        }

        if (base.Dispatch(args)) return false;
        return true;
    }

    protected override void OnStart()
    {
        base.OnStart();

        aliases = JsonService.LoadJson<List<AliasItem>>("config", "alias.json");
        if (aliases == null) aliases = new List<AliasItem>();
    }

    protected override void DrawMenu()
    {
        if (aliases!.Count > 0)
            Print.Line($"Your marcos ({aliases.Count}):");
        else
        {
            Print.Line($"Empty aliases.", ConsoleColor.Yellow);
            return;
        }

        for (int i = 0; i < aliases.Count; i++)
        {
            Markup.Rich($"({i}) [cyan]{aliases[i].name}[white]: " +
                         aliases[i].alias + "\n", null!);
        }
    }

    protected override void AfterInput(string input)
    {
        switch (input)
        {
            case "add": AddAlias(); break;
            case "remove": RemoveAlias(); break;
            default: return;
        }
    }

    protected override void OnExit()
    {
        base.OnExit();
        JsonService.SaveJson("config", "alias.json", aliases!);
    }
}