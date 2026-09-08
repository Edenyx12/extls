using extls.Core;
using extls.Core.Decoration;
using System.Diagnostics;

namespace extls.Tools;

public record DirConfig(bool icons, bool mini);

[ModuleName("dir")]
public partial class Dir : Module
{
    private DirConfig config;

    public Dir()
    {
        name = "dir";
        version = "0.6.5b";
        commands =
        [
            new HelpSlot("tree", "scans the specified directory",
                      new[] {"\"--recursive, -r\" - recursive path scanning",
                             "\"--summary, --sum, -s\" - scan quantity only",
                             "\"--type file\\folder\\all\" - specify scanning of folders, files, or both (default: all)"},
                             "\"extls dir tree \"YOUR_PATH_ON_DISK\" -s --type folder\""),
                new HelpSlot("create", "creates files\\folders on the disk at the specified path or relative to the terminal's current path",
                      new[] {"\"--set-path, -sp\" - manually specify the full path (e.g., \"-sp Disk:\\\")"},
                             "\'extls dir create file\\\\folder \"file.txt\" -sp \"PATH\"\'")
        ];

        string configPath = Path.Combine(Root.RootPath, "config", "dir-config.json");

        if (!File.Exists(configPath))
            config = new(false, false);
        else
        {
            config = JsonService.LoadJson<DirConfig>(Path.Combine(Root.RootPath, "config"), "dir-config.json");
            Print.Debug("dir: config success loaded.");
        }

        Config();
    }

    [MethodName(Params.Args, ["create", "c"])]
    public void Create(string[] args)
    {
        if (args.Length == 0) { Print.Error("Arguments are missing."); return; }

        int createType = args[0] switch { "file" => 1, "folder" => 0, _ => -1, };
        if (createType == -1) { Print.Error("invalid type for creation"); return; }
        if (args.Length < 2) { Print.Error("invalid name for creation"); }
        string name = args[1];

        bool auto = true;
        string userPath = "";

        for (int i = 0; i < args.Length; i++)
        {
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

        if (success) Markup.Rich($"Item created **$[green]successfully[white]**!" +
                                 $"\n$[white]Item name: $[cyan]{Markup.FixBackslash(name)}" +
                                 $"\n$[white]Path: $[yellow]{Markup.FixBackslash(path)}");
        else Markup.Rich($"Create item **$[red]failed$[white]** of reason: $[yellow]{failReason}$[yellow]." +
                         $"\n$[white]Item name: **$[cyan]{Markup.FixBackslash(name)}**" +
                         $"\n$[white]Path: *$[yellow]{Markup.FixBackslash(path)}*");
    }

    [MethodName(Params.Args, ["tree", "tr"])]
    public void Tree(string[] args)
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
                    recursiveLevels = 999999;
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out int l))
                    {
                        recursiveLevels = l;
                        continue;
                    }
                    break;
                case "--summary" or "--sum" or "-s": summary = true; break;
                case "--type":
                    if (i + 1 < args.Length)
                        foldersOrFilesOrAll = args[i + 1] switch
                        {
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

        if (!Directory.Exists(path))
        { 
            Print.Error("Unknown path."); 
            Print.Debug(path);
            return; 
        }

        if (!config.mini)
            Markup.Rich($"Scan result of path $[{
                (Root.Platform is Platform.Windows ? "yellow" : "#7dbeff")
                }]'{Markup.FixBackslash(path)}'$[white]:\n", true);
        else Console.WriteLine();

        Stopwatch time = Stopwatch.StartNew();

        int sumDir = 0;
        int sumF = 0;

        ExecuteTree(path, 0, (recursive ? recursiveLevels + 1 : 1), summary, foldersOrFilesOrAll, ref sumDir, ref sumF);

        if (summary || Print.verbose)
        {
            if (foldersOrFilesOrAll is 0 or 1) Markup.Rich($"Folders $[green]scanned$[white]: {sumDir}\n");
            if (foldersOrFilesOrAll is 0 or 2) Markup.Rich($"Files $[green]scanned$[white]: {sumF}\n");
        }

        time.Stop();
        if (Print.verbose)
            Markup.Rich($"\nScanned in: $[#d375ff]{time.Elapsed}", true);
        Console.WriteLine();
    }

    [MethodName(Params.None, ["grid", "gr", "grd"])]
    public void Grid()
    {
        string path = Directory.GetCurrentDirectory();

        List<GridItem> items = new();

        foreach (string folder in Directory.EnumerateDirectories(path)) items.Add(new(Path.GetFileName(folder), isFolder: true));
        foreach (string file in Directory.EnumerateFiles(path)) items.Add(new(Path.GetFileName(file), isFolder: false));

        if (items.Count == 0)
        {
            Print.Warning("The current folder is empty.");
            return;
        }

        Console.WriteLine();

        items.Sort((a, b) => b.name.Length.CompareTo(a.name.Length));

        for (int s = 0; s < items.Count; s++)
        {
            string paddedName = items[s].name.PadRight(items[0].name.Length);
            items[s] = new GridItem(paddedName, items[s].isFolder);
        }

        items.Sort((a, b) =>
        {
            int folderCompare = b.isFolder.CompareTo(a.isFolder);
            
            if (folderCompare != 0) return folderCompare;
            
            return string.Compare(a.name, b.name, StringComparison.Ordinal);
        });

        foreach (GridItem item in items)
            Print.Debug($"{item.name} ({item.name.Length} chars.)");
        Print.Debug("\n");

        int width = Console.WindowWidth;
        int lengthCount = 0;

        for (int i = 0; i < items.Count; i++)
        {
            FileIcon(items[i].name, ref fileIconUsable);
            string output = config.icons 
                ? $"{(items[i].isFolder ? "" : fileIconUsable.icon)} {items[i].name}  "
                : $"{items[i].name}  ";

            if (lengthCount + output.Length > width) 
            {
                lengthCount = 0;
                Console.Write("\n");
            }

            lengthCount += output.Length;

            Markup.Rich($"$[{
                (items[i].isFolder ? (Root.Platform is Platform.Linux ? "#7dbeff" : "yellow") : fileIconUsable.color)
                }]{output}");
        }
    }

    [MethodName(Params.None, "config")]
    public void Config()
    {
        string path = Path.Combine(Root.RootPath, "config", "dir-config.json");
        string fixedConsolePath = string.Empty;

        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] is '\\') fixedConsolePath += '\\';
            fixedConsolePath += path[i];
        }

        if (!File.Exists(path))
        {
            JsonService.SaveJson(Path.Combine(Root.RootPath, "config"), "dir-config.json", config);
            Markup.Rich($"dir: config $[green]successfully$[white] created in: \n $[yellow]{fixedConsolePath}"
                        + "\n$[darkgray]to change config, open config.json");
        }
        else Print.Debug($"dir: config already exists in {path}");
    }
}