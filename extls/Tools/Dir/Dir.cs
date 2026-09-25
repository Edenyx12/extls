using extls.Core;
using extls.Core.Decoration;
using System.Diagnostics;

namespace extls.Tools;

public record DirConfig(bool icons, bool mini, string[] ignoreExtensions)
{
    public bool IsIgnored(ReadOnlySpan<char> extension)
    {
        for (int i = 0; i < ignoreExtensions.Length; i++)
        {
            if (extension.SequenceEqual(ignoreExtensions[i]))
                return true;
        }

        return false;
    }
}

[ModuleName("dir")]
public partial class Dir : Module
{
    private DirConfig config = new(false, false, [".ignoreextension"]);

    public Dir()
    {
        name = "dir";
        version = "0.6.9b";

        string path = Path.Combine(Global.RootPath, "config", "dir-config.json");

        if (File.Exists(path))
        {
            config = JsonService.LoadJson<DirConfig>(
                Path.Combine(Global.RootPath, "config"),
                "dir-config.json");
        }
        else
        {
            string fixedConsolePath = string.Empty;

            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] is '\\') fixedConsolePath += '\\';
                fixedConsolePath += path[i];
            }

            JsonService.SaveJson(
                Path.Combine(Global.RootPath, "config"),
                "dir-config.json",
                config);

            Markup.Rich($"dir: config $[green]successfully$[white] created in: \n $[yellow]{fixedConsolePath}" +
                        "\n$[darkgray]to change config, open config.json\n");
        }
    }

    [MethodName(["create", "c"])]
    public void Create(string[] args)
    {
        if (args.Length == 0) { Out.Error("Arguments are missing."); return; }

        int createType = args[0] switch { "file" => 1, "folder" => 0, _ => -1, };
        if (createType == -1) { Out.Error("Invalid type for creation."); return; }
        if (args.Length < 2) { Out.Error("Invalid name for creation."); }
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
        if (!Path.Exists(path)) { Out.Error("Unknown path."); return; }

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
                                 $"\n$[white]Item name: $[cyan]{Markup.SafeBackslash(name)}" +
                                 $"\n$[white]Path: $[yellow]{Markup.SafeBackslash(path)}");
        else Markup.Rich($"Create item **$[red]failed$[white]** of reason: $[yellow]{failReason}$[yellow]." +
                         $"\n$[white]Item name: **$[cyan]{Markup.SafeBackslash(name)}**" +
                         $"\n$[white]Path: *$[yellow]{Markup.SafeBackslash(path)}*");
    }

    [MethodName(["tree", "tr"])]
    public void Tree()
    {
        bool recursive = Arguments.Get("r", "recursive");

        if (int.TryParse(Arguments.GetRight("r", "recursive"), out int recursiveLevels)){}
        else recursiveLevels = recursive ? 999999 : 1;
        
        bool summary = Arguments.Get("s", "sum", "summary");
        byte typeFilter = Arguments.GetRight("t", "type") switch {"file" => 2, "folder" => 1, _ or "all" => 0};

        string? path = Arguments.GetPath();

        if (path is null)
        {
            path = Directory.GetCurrentDirectory();
            return;
        }

        if (!config!.mini)
            Markup.Rich($"Scan result of path $[{
                (Global.Platform is Platform.Windows ? "yellow" : "#7dbeff")
            }]'{Markup.SafeBackslash(path)}'$[white]:\n", true);
        else Console.WriteLine();

        Stopwatch time = Stopwatch.StartNew();

        int summaryFolders = 0;
        int summaryFiles = 0;

        ExecuteTree(
            path: path,
            currentLevel: 0,
            maxLevels: recursive ? recursiveLevels + 1 : 1,
            summary: summary,
            typeFilter: typeFilter,
            summaryFolders: ref summaryFolders,
            summaryFiles: ref summaryFiles
        );

        if (summary || Global.Verbose)
        {
            if (typeFilter is 0 or 1) Markup.Rich($"Folders $[green]scanned$[white]: {summaryFolders}\n");
            if (typeFilter is 0 or 2) Markup.Rich($"Files $[green]scanned$[white]: {summaryFiles}\n");
        }

        time.Stop();
        if (Global.Verbose) Markup.Rich($"\nScanned in: $[#d375ff]{time.Elapsed}", true);
        Console.WriteLine();
    }

    [MethodName(["grid", "gr", "grd"])]
    public void Grid()
    {
        string path = Directory.GetCurrentDirectory();

        List<GridItem> items = new();

        foreach (string folder in Directory.EnumerateDirectories(path)) items.Add(new(Path.GetFileName(folder), isFolder: true));
        foreach (string file in Directory.EnumerateFiles(path))
        {
            string f = Path.GetFileName(file);
            if (config.IsIgnored(Path.GetExtension(f))) continue;
            else items.Add(new(f, isFolder: false));
        }

        if (items.Count == 0)
        {
            Out.Warning("The current folder is empty.");
            return;
        }

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
            Out.Debug($"{item.name} ({item.name.Length} chars.)");
        Out.Debug("\n");

        int width = Console.WindowWidth;
        int lengthCount = 0;

        for (int i = 0; i < items.Count; i++)
        {
            FileIcon(items[i].name, ref fileIconUsable);
            string output = config!.icons
                ? $"{(items[i].isFolder ? "" : fileIconUsable.icon)} {items[i].name}  "
                : $"{items[i].name}  ";

            if (lengthCount + output.Length > width)
            {
                lengthCount = 0;
                Console.Write("\n");
            }

            lengthCount += output.Length;

            Markup.Rich($"$[{(items[i].isFolder ? (Global.Platform is Platform.Linux ? "#7dbeff" : "yellow") : fileIconUsable.color)}]{output}");
        }
    }


    [MethodName("config")]
    public void Config()
    {
        string path = Path.Combine(Global.RootPath, "config", "dir-config.json");

        if (Arguments.Get("c", "clear"))
            if (File.Exists(path)) File.Delete(path);        

        bool ingoreExtensions = config.ignoreExtensions.Length > 0;
        string extensions = string.Empty;

        if (ingoreExtensions)
        {
            foreach (string extension in config.ignoreExtensions)
                extensions += $"  $[magenta]{extension}\n";
        }
        else
        {
            extensions = "$[yellow]Empty";
        }

        Markup.Rich($"$[cyan]Icons: {(config.icons ? "$[green]True" : "$[red]False")}\n" +
                    $"$[cyan]Mini: {(config.mini ? "$[green]True" : "$[red]False")}\n" +
                    $"$[cyan]Ignore Extensions: $[white]\n{extensions}");
    }
}