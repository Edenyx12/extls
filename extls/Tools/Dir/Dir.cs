using extls.Core;
using System.Diagnostics;

namespace extls.Tools
{
    [ModuleName("dir")]
    public partial class Dir : Module
    {
        public Dir()
        {
            name = "dir";
            version = "0.6.2b";
            commands = new[]
            {
                new HelpSlot("scan", "scans the specified directory",
                      new[] {"\"--recursive, -r\" - recursive path scanning",
                             "\"--summary, --sum, -s\" - scan quantity only",
                             "\"--type file\\folder\\all\" - specify scanning of folders, files, or both (default: all)"},
                             "\"extls dir scan \"YOUR_PATH_ON_DISK\" -s --type folder\""),
                new HelpSlot("create", "creates files\\folders on the disk at the specified path or relative to the terminal's current path",
                      new[] {"\"--set-path, -sp\" - manually specify the full path (e.g., \"-sp Disk:\\\")"},
                             "\'extls dir create file\\\\folder \"file.txt\" -sp \"PATH\"\'")
            };
        }

        public override bool Dispatch(string[] args)
        {
            if (base.Dispatch(args)) return false;

            switch (args[0])
            {
                case "scan": Scan(Utils.RemoveZeroCommand(args)); break;
                case "create": Create(Utils.RemoveZeroCommand(args)); break;
                default:
                    Utils.InvalidOperation();
                    return false;
            }

            return true;
        }
        
        public void Create(string[] args)
        {
            if (args.Length == 0) { Print.Error("Arguments are missing."); return; }

            int createType = args[0] switch { "file" => 1, "folder" => 0, _ => -1, };
            if (createType == -1) { Print.Error("invalid type for creation"); return; }
            if (args.Length < 2) { Print.Error("invalid name for creation"); }
            string name = args[1];

            bool auto = true;
            string userPath = "";

            for (int i = 0; i < args.Length; i++) {
                switch (args[i])
                {
                    case "--set-path" or "-sp":
                        if (i + 1 > args.Length) break;
                        auto = false;
                        userPath = args[i + 1];
                        break;
                }
            }

            string path = auto ? Directory.GetCurrentDirectory() : userPath;
            if (!Path.Exists(path)) { Print.Error("Unknown path."); return; }

            bool success = true;
            string failReason = "";
            try
            {
                string fullpath = Path.Combine(path, name);
                switch (createType)
                {
                    case 0:
                        Directory.CreateDirectory(fullpath);
                        break;
                    case 1:
                        File.Create(fullpath);
                        break;

                    default: throw new Exception();
                }
            }
            catch (Exception ex) { success = false; failReason = Reason(ex); }

            if (success) Markup.Rich($"Item created [green]successfully[white]!" +
                                     $"\n[white]Item name: [cyan]{Markup.FixBackslash(name)}" +
                                     $"\n[white]Path: [yellow]{Markup.FixBackslash(path)}", null!);
            else Markup.Rich($"Create item [red]failed[white] of reason: [yellow]{failReason}[yellow]." +
                             $"\n[white]Item name: [cyan]{Markup.FixBackslash(name)}" +
                             $"\n[white]Path: [yellow]{Markup.FixBackslash(path)}", null!);
        }

        public void Scan(string[] args)
        {
            if (args.Length == 0)
            {
                Print.Error("Arguments are missing.");
                return;
            }

            bool recursive = false;
            int recursiveLevels = 1;
            bool summary = false;
            byte foldersOrFilesOrAll = 0; // 0 - all, 1 - folder, 2 - file

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--recursive" or "-r": 
                        recursive = true;
                        recursiveLevels = int.MaxValue;
                        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int l))
                            recursiveLevels = l;
                        break;
                    case "--summary" or "--sum" or "-s":  summary = true; break;
                    case "--type": 
                        if (i + 1 < args.Length)
                            foldersOrFilesOrAll = args[i + 1] switch {
                                "file" => 2,
                                "folder" => 1,
                                "all" => 0,
                                _ => 0
                            };
                        break;
                }
            }

            string path = args[0];
            
            if (args[0] is ".") path = Directory.GetCurrentDirectory();
            else if (args.Length > 0 && args[0][0] is '.')
                path = Path.GetFullPath(args[0], Directory.GetCurrentDirectory());
            
            if (!Directory.Exists(path)) { Print.Error("Unknown path."); return; }
            
            Markup.Rich($"Scan result of path [yellow]'{Markup.FixBackslash(path)}'[white]:\n", null!, true);

            Stopwatch time = Stopwatch.StartNew();

            int sumDir = 0;
            int sumF = 0;
            
            ExecuteScan(path, 0, (recursive ? recursiveLevels : 1), summary, foldersOrFilesOrAll, ref sumDir, ref sumF);

            if (summary || Print.verbose)
            {
                if (foldersOrFilesOrAll is 0 or 1) Markup.Rich($"Folders [green]scanned[white]: {sumDir}\n", null!);
                if (foldersOrFilesOrAll is 0 or 2) Markup.Rich($"Files [green]scanned[white]: {sumF}\n", null!);
            }

            time.Stop();
            if (Print.verbose)
                Markup.Rich($"\nScanned in: [magenta]{time.Elapsed}", null!, true);
            Console.WriteLine();
        }
        private void ExecuteScan(string path, int currentLevel, int maxLevels, bool summary, byte typeFilter, ref int sumDir, ref int sumF)
        {
            FolderStatus status = OpenFolder(path);

            if (!summary)
            {
                string indent = new string(' ', currentLevel * 2);
                string folderName = System.IO.Path.GetFileName(path);
                if (string.IsNullOrEmpty(folderName)) folderName = path;

                string statusTag = status switch
                {
                    FolderStatus.Ok => "",
                    FolderStatus.Empty => "[[cyan](empty)[white]]",
                    FolderStatus.AccessDenied => "[[red](access denied)[white]]",
                    FolderStatus.NotFound => "[[yellow](not found)[white]]",
                    _ => "[[red](error)[white]]"
                };
                
                if (Root.Platform is Platform.Windows)
                    Markup.Rich($"{indent}[yellow] {folderName}\\\\ {statusTag}", null!, true);
                else
                {
                    if (folderName is "/")
                        Markup.Rich($"{indent}[yellow]{folderName} {statusTag}", null!, true);
                    else Markup.Rich($"{indent}[yellow] {folderName}/ {statusTag}", null!, true);
                }
            }

            if (status != FolderStatus.Ok) return;

            // FILE
            if (typeFilter is 0 or 2)
            {
                try
                {
                    foreach (string file in Directory.EnumerateFiles(path))
                    {
                        if (currentLevel < maxLevels) sumF++;
                        if (!summary && currentLevel < maxLevels)
                        {
                            string indent = new string(' ', (currentLevel + 1) * 2);
                            string fileName = System.IO.Path.GetFileName(file);
                            FileIcon(fileName, ref fileIconUsable);
                            
                            Markup.Rich($"{indent}" +
                                        $"[{fileIconUsable.color}]" +
                                        $"{fileIconUsable.icon}" +
                                        $" {fileName}", null!, true);
                        }
                    }
                }
                catch { }
            }
            
            // FOLDER
            if (typeFilter is 0 or 1)
            {
                if (currentLevel < maxLevels)
                {
                    try
                    {
                        foreach (string subDir in Directory.EnumerateDirectories(path))
                        {
                            sumDir++;
                            ExecuteScan(subDir, currentLevel + 1, maxLevels, summary, typeFilter, ref sumDir, ref sumF);
                        }
                    }
                    catch { }
                }
            }
        }
    }
}
