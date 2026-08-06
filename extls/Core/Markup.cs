namespace extls.Core
{
    public static class Markup
    {
        static Markup()
        {
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        }

        public static void Rich(string code, string[] vars, bool line = false)
        {
            string[] tokens = Parse(code);
            bool openCommand = false;

            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "\\" && i + 1 < tokens.Length)
                {
                    Print.Inline(tokens[i + 1]);
                    i++;
                    continue;
                }

                openCommand = tokens[i] switch {  
                    "[" => true,
                    "]" => false,
                    _ => openCommand
                };
                if (tokens[i] is "[" or "]") continue;
               
                if (openCommand)
                {
                    if (i + 3 < tokens.Length && tokens[i] is "v" && tokens[i + 1] is "(" && tokens[i + 3] is ")")
                    {
                        if (int.TryParse(tokens[i + 2], out int idx) && idx >= 0 && idx < vars.Length)
                            Print.Inline(vars[idx]);
                        i += 3;
                    }
                    else Console.ForegroundColor = ChangeColor(tokens[i]);

                    continue;
                }

                Print.Inline(tokens[i]);
            }

            Console.ResetColor();
            if (line) Console.WriteLine();
        }
        public static string FixBackslash(string text)
        {
            string fix = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] is '\\')
                    fix += '\\';
                fix += text[i];
            }

            return fix;
        }

        private static string[] Parse(string code)
        {
            var tokens = new List<string>();
            string raw = "";

            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\\')
                {
                    if (raw != "") { tokens.Add(raw); raw = ""; }

                    tokens.Add("\\");

                    if (i + 1 < code.Length)
                    {
                        tokens.Add(code[i + 1].ToString());
                        i++;
                    }
                    continue;
                }

                if (code[i] is '[' or ']' or '(' or ')')
                {
                    if (raw != "") { tokens.Add(raw); raw = ""; }
                    tokens.Add(code[i].ToString());
                    continue;
                }

                raw += code[i];
            }

            if (raw != "") tokens.Add(raw);
            return tokens.ToArray();
        }
        private static ConsoleColor ChangeColor(string color) => color.ToLower() switch {
            "black" => ConsoleColor.Black,
            "darkblue" => ConsoleColor.DarkBlue,
            "darkgreen" => ConsoleColor.DarkGreen,
            "darkcyan" => ConsoleColor.DarkCyan,
            "darkred" => ConsoleColor.DarkRed,
            "darkmagenta" => ConsoleColor.DarkMagenta,
            "darkyellow" => ConsoleColor.DarkYellow,
            "gray" => ConsoleColor.Gray,

            "darkgray" => ConsoleColor.DarkGray,
            "blue" => ConsoleColor.Blue,
            "green" => ConsoleColor.Green,
            "cyan" => ConsoleColor.Cyan,
            "red" => ConsoleColor.Red,
            "magenta" => ConsoleColor.Magenta,
            "yellow" => ConsoleColor.Yellow,
            "white" => ConsoleColor.White,

            _ => ConsoleColor.White
        };
    }
}
