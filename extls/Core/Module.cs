namespace extls.Core
{
    public abstract class Module
    {
        public string? name;
        public string? version;
        protected HelpSlot[]? commands;

        public virtual void Help()
        {
            Markup.Rich($"Help of module [blue]'{name}'[white]:\n\n", null!);
            for (int i = 0; i < commands?.Length; i++)
            {
                string wrap = "\n";
                if (i + 1 == commands?.Length) wrap = "";
                Markup.Rich($"* [yellow]{commands?[i].name}[white] - {commands?[i].description}" + wrap, null!);

                if (commands?[i].args != null && commands[i].args.Length > 0)
                {
                    foreach (string arg in commands[i].args)
                    {
                        if (arg is null or "") continue;
                        Print.Inline($"\n  {arg}", ConsoleColor.DarkGray);
                    }
                }

                if (!(commands?[i].example is null or ""))
                    Markup.Rich($"\n  [darkgray]example: {commands[i].example}.", null!);
            }
            Console.WriteLine("\n");
        }
        public virtual void Version() => Markup.Rich(
            $"Module '[blue]{name}'" +
            $"\n[white]Version: [cyan]{version}.", 
            null!);

        public virtual bool Dispatch(string[] args)
        {
            if (args.Length <= 0 || (args[0] is "-v" or "--version"))
            {
                Version();
                return true;
            }

            if (args[0] is "help" or "-h" or "--help")
            {
                Help();
                return true;
            }

            return false;
        }
    }
    public struct HelpSlot
    {
        public string name;
        public string description;
        public string[] args;
        public string example;

        public HelpSlot(string name, string description, string[] args, string example)
        {
            this.name = name;
            this.description = description;
            this.args = args;
            this.example = example;
        }
    }
}
