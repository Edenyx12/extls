using extls.Core;
using System.Diagnostics;

namespace extls.Tools
{
    public class Alias : Module
    {
        private List<AliasItem>? aliases { get; set; }
        public Alias()
        {
            name = "alias";
            version = "0.1a";
            commands = null!;
        }
        
        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;

            aliases = JsonService.LoadJson<List<AliasItem>>("config", "alias.json");
            if (aliases == null)
                aliases = new List<AliasItem>();
          
            switch (args[0])
            {
                case "add": AddAlias(); break;
                case "list" or "show": ShowAlias(); break;
                case "remove" or "rm": RemoveAlias(); break;
                default:
                    ExecuteAlias(args);
                    break;
            }

            JsonService.SaveJson<List<AliasItem>>("config", "alias.json", aliases);
            return true;
        }

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

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{command}\"",

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
            ShowAlias();

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

        private void ShowAlias()
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

    public record AliasItem(string name, string alias);
}