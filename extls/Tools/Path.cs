using extls.Core;
using Parlot.Fluent;

namespace extls.Tools
{
    public class Path : Module
    {
        public Path()
        {
            name = "path";
            version = "0.2a";
            commands = new[]
            {
                new HelpSlot("scan", "scans the specified directory",
                      new[] {"\"--recursive, -r\" - recursive path scanning",
                             "\"--summary, --sum, -s\" - scan quantity only",
                             "\"--type file\\folder\\all\" - specify scanning of folders, files, or both (default: all)"},
                             "\"extls path scan \"YOUR_PATH_OF_DISK\" -s --type folder\"")
            };
        }

        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;

            switch (args[0])
            {
                case "scan": Scan(Utils.RemoveZeroCommand(args)); break;
                default:
                    Utils.InvalidOperation();
                    return false;
            }

            return true;
        }

        private void Scan(string[] args)
        {
            if (args.Length == 0)
            {
                Print.Error("Arguments are missing.");
                return;
            }

            bool recursive = false;
            bool summary = false;
            byte foldersOrFilesOrAll = 0; // 0 - all, 1 - folder, 2 - file

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--recursive" or "-r": recursive = true; break;
                    case "--summary" or "--sum" or "-s": summary = true; break;
                    case "--type": if (i + 1 < args.Length)
                            foldersOrFilesOrAll = args[i + 1] switch {
                                "file" => 2,
                                "folder" => 1,
                                "all" => 0,
                                _ => 0
                            };
                        break;
                }
            }
            if (!Directory.Exists(args[0])) { Print.Error("Unknown path."); return; }

            Print.Inline($"\nScan result of "); 
            Print.Inline($"'{args[0]}'", ConsoleColor.Yellow); 
            Print.Inline(" path" + (recursive ? " (recursive)" : "") + ":\n\n");

            int sumDir = 0;
            int sumF = 0;

            var foldersToScan = new Stack<string>();
            foldersToScan.Push(args[0]);

            while (foldersToScan.Count > 0)
            {
                string currentFolder = foldersToScan.Pop();

                int currentDepth = currentFolder
                    .Replace(args[0], "")
                    .Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                    .Length;

                string indent = new string(' ', currentDepth * 4);

                // FILES
                if (foldersOrFilesOrAll is 0 or 2)
                {
                    try
                    {
                        foreach (string f in Directory.EnumerateFiles(currentFolder))
                        {
                            sumF += 1;

                            if (!summary)
                            {
                                string relativeFilePath = System.IO.Path.GetRelativePath(args[0], f);

                                int fileDepth = relativeFilePath.
                                    Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
                                    .Length - 1;
                                string fileIndent = new string(' ', fileDepth * 4);

                                Print.Line($"{fileIndent}\\{relativeFilePath}");
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                }
                // FOLDERS
                if (foldersOrFilesOrAll is 0 or 1)
                {
                    try
                    {
                        foreach (string dir in Directory.EnumerateDirectories(currentFolder))
                        {
                            string status = "";
                            try { status = !System.Linq.Enumerable.Any(Directory.EnumerateFileSystemEntries(dir)) ? " (empty)" : ""; }
                            catch (UnauthorizedAccessException) { status = " (access denied)"; }

                            string color = status switch { " (empty)" => "yellow", " (access denied)" => "red", _ => "white" };

                            sumDir += 1;

                            if (!summary)
                            {
                                string relativeDirPath = System.IO.Path.GetRelativePath(args[0], dir);

                                int dirDepth = relativeDirPath.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length - 1;
                                string dirIndent = new string(' ', dirDepth * 4);

                                Markup.Rich($"{dirIndent}\\{relativeDirPath} [{color}]{status}\n", null!);
                            }

                            if (recursive && status != " (access denied)")
                                foldersToScan.Push(dir);
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                }
                if (!recursive) foldersToScan.Clear();
            }

            if (summary)
            {
                if (foldersOrFilesOrAll is 0 or 1) Markup.Rich($"Folders [green]scanned[white]: {sumDir}\n", null!);
                if (foldersOrFilesOrAll is 0 or 2) Markup.Rich($"Files [green]scanned[white]: {sumF}\n", null!);
            }
            Console.WriteLine();
        }
    }
}
