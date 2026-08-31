namespace extls.Core
{
    public abstract class Module
    {
        public string? name;
        public string? version;
        protected HelpSlot[]? commands;

        public virtual void Help()
        {
            
            Markup.Rich($"Help of module [blue]'{name}'[white]:\n", null!, true);
            for (int i = 0; i < commands?.Length; i++)
            {
                bool isArgs = commands?[i].args != null && commands[i].args.Length > 0;
                bool isExample = !(commands?[i].example is null or "");
                Markup.Rich((isArgs && isExample && i != 0 ? "\n" : "") + 
                    $"* [yellow]{commands?[i].name}[white] - {commands?[i].description}", null!, true);

                if (isArgs)
                {
                    foreach (string arg in commands?[i].args!)
                    {
                        if (arg is null or "") continue;
                        Print.Line($"  {arg}", ConsoleColor.DarkGray);
                    }
                }

                if (isExample)
                    Markup.Rich($"  [darkgray]example: {commands?[i].example}.", null!, true);
            }
            Console.WriteLine("\n");
        }
        public virtual void Version() => Markup.Rich(
            $"\nModule '[blue]{name}'" +
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
