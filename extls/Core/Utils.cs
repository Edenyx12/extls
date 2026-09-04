namespace extls.Core
{
    public static class Utils
    {
        public static void InvalidOperation() => Print.Error(
            "Fatal error! Failed to execute this command. Use '--help' for assistance.");

        public static string[] RemoveZeroCommand(string[] args)
        {
            if (args == null || args.Length <= 1) return Array.Empty<string>();
            
            string[] newArgs = new string[args.Length - 1];
            Array.Copy(args, 1, newArgs, 0, newArgs.Length);
            return newArgs;
        }
    }
}
