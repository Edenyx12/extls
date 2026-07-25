namespace extls.Core
{
    public static class Utils
    {
        public static void InvalidOperation() => Print.Error(
            "Fatal error! Failed to execute this command. Use '--help' for assistance.");

        public static string[] RemoveZeroCommand(string[] args)
        {
            string[] newArgs = new string[args.Length - 1];

            for (int i = 1; i < args.Length; i++)
                newArgs[i-1] = args[i];

            return newArgs;
        }
    }
}
