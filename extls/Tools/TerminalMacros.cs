using extls.Core;
using System.Diagnostics;

namespace extls.Tools
{
    public class TerminalMacros : Module
    {
        private List<MacroItem>? macros { get; set; }
        public TerminalMacros()
        {
            name = "terminal-macros";
            version = "0.1a";
            commands = null!;
        }
        
        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;

            macros = JsonService.LoadJson<List<MacroItem>>("config", "macro.json");
            if (macros == null)
                macros = new List<MacroItem>();
          
            switch (args[0])
            {
                case "add": AddMacro(); break;
                case "list" or "show": ShowMacro(); break;
                case "remove" or "rm": RemoveMacro(); break;
                default:
                    ExecuteMacro(args);
                    break;
            }

            JsonService.SaveJson<List<MacroItem>>("config", "macro.json", macros);
            return true;
        }

        private void ExecuteMacro(string[] args)
        {
            if (args.Length <= 1)
            {
                Utils.InvalidOperation();
                return;
            }

            MacroItem macroItem = null!;
            if (CheckMacro(args[0], out int index))
                macroItem = macros![index];

            if (macroItem == null)
            {
                Print.Error($"Invalid macro name of '{args[0]}'");
                return;
            }

            string command = LinkMacro(macroItem.macro, args[1]);

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

        private void AddMacro()
        {
            Print.Inline("Enter the name macro > ");
            string name = Console.ReadLine()!;

            Print.Inline("Enter the macro with '&' symbol of target arg > ");
            string macro = Console.ReadLine()!;

            if (CheckMacro(name, out int i))
            {
                Print.Error($"Already used name of '{name}'");
                return;
            }

            int bindingChars = 0;
            for (int j = 0; j < macro.Length; j++)
            {
                if (macro[j] == '&')
                    bindingChars++;
            }

            if (bindingChars > 1)
            {
                Print.Error("You can use only one '&' on macro.");
                return;
            }

            MacroItem macroItem = new MacroItem(name, macro);

            macros!.Add(macroItem);
            Markup.Rich("Macro sucessfully created: \n"
                      + $"[cyan]name[white]: {name}\n"
                      + $"[magenta]macro[white]: {macro}", null!);
        }

        private void RemoveMacro()
        {
            ShowMacro();

            Print.Inline("\n\nEnter the number of delete (enter for return) > ");
            string input = Console.ReadLine()!;
            if (input == "") return;

            if (int.TryParse(input, out int index))
            {
                if (index >= macros!.Count)
                {
                    Print.Error("Invalid index.");
                    return;
                }

                Print.Line($"Successfully deleted macro '{macros[index].name}'",
                          ConsoleColor.Green);
                macros.Remove(macros[index]);
            }
        }

        private void ShowMacro()
        {
            if (macros!.Count > 0)
                Print.Line($"Your marcos ({macros.Count}):");
            else 
            {
                Print.Line($"Empty macros.", ConsoleColor.Yellow);
                return;
            }

            for (int i = 0; i < macros.Count; i++)
            {
                Markup.Rich($"({i}) [cyan]{macros[i].name}[white]: " +
                             macros[i].macro + "\n", null!);
            }
        }

        private bool CheckMacro(string name, out int index)
        {
            index = -1;
            for (int i = 0; i < macros!.Count; i++)
            {
                if (macros[i].name == name)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }
        private string LinkMacro(string macro, string command)
        {
            string linked = "";
          
            for (int i = 0; i < macro.Length; i++)
            {
                if (macro[i] == '&')
                {
                    linked += command;
                    continue;
                }
                linked += macro[i];
            }

            return linked;
        }
    }

    public record MacroItem(string name, string macro);
}