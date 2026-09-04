using extls.Core;
using System.Diagnostics;

namespace extls.Tools;

public partial class Alias
{
    private List<AliasItem>? aliases { get; set; }

    private void ExecuteAlias(string[] args)
    {
        if (args.Length <= 1)
        {
            Utils.InvalidOperation();
            return;
        }

        AliasItem aliasItem = null!;
        if (CheckAlias(args[0], out int index))
            aliasItem = aliases![index];

        if (aliasItem == null)
        {
            Print.Error($"Invalid alias name of '{args[0]}'");
            return;
        }

        string command = LinkAlias(aliasItem.alias, args[1]);

        bool isLinux = Root.Platform is Platform.Linux;

        var psi = new ProcessStartInfo
        {
            FileName = isLinux ? "/bin/bash" : "cmd.exe",
            Arguments = isLinux ? $"-c \"{command}\"" : $"/c \"{command}\"",

            UseShellExecute = false,
            CreateNoWindow = false,

            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false
        };

        using var process = Process.Start(psi);
        if (process == null)
        {
            Utils.InvalidOperation();
            return;
        }

        process.WaitForExit();

        JsonService.SaveJson<List<AliasItem>>("config", "alias.json", aliases);
    }

    private void AddAlias()
    {
        Print.Inline("Enter the name alias > ");
        string name = Console.ReadLine()!;

        Print.Inline("Enter the alias with '&' symbol of target arg > ");
        string alias = Console.ReadLine()!;

        if (CheckAlias(name, out int i))
        {
            Print.Error($"Already used name of '{name}'");
            return;
        }

        int bindingChars = 0;
        for (int j = 0; j < alias.Length; j++)
        {
            if (alias[j] == '&')
                bindingChars++;
        }

        if (bindingChars > 1)
        {
            Print.Error("You can use only one '&' on alias.");
            return;
        }

        AliasItem aliasItem = new AliasItem(name, alias);

        aliases!.Add(aliasItem);
        Markup.Rich("Alias sucessfully created: \n"
                  + $"[cyan]name[white]: {name}\n"
                  + $"[magenta]alias[white]: {alias}", null!);
    }

    private void RemoveAlias()
    {
        Print.Inline("\n\nEnter the number of delete (enter for return) > ");
        string input = Console.ReadLine()!;
        if (input == "") return;

        if (int.TryParse(input, out int index))
        {
            if (index >= aliases!.Count)
            {
                Print.Error("Invalid index.");
                return;
            }

            Print.Line($"Successfully deleted alias '{aliases[index].name}'",
                      ConsoleColor.Green);
            aliases.Remove(aliases[index]);
        }
    }

    private bool CheckAlias(string name, out int index)
    {
        index = -1;
        for (int i = 0; i < aliases!.Count; i++)
        {
            if (aliases[i].name == name)
            {
                index = i;
                return true;
            }
        }
        return false;
    }
    private string LinkAlias(string alias, string command)
    {
        string linked = "";

        for (int i = 0; i < alias.Length; i++)
        {
            if (alias[i] == '&')
            {
                linked += command;
                continue;
            }
            linked += alias[i];
        }

        return linked;
    }
}