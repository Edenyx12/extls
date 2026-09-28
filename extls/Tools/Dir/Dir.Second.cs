using System.Text;
using extls.Core;
using extls.Core.Decoration;

namespace extls.Tools;

public partial class Dir
{
    private void ExecuteTree(string path, int currentLevel, int maxLevels, bool summary, byte typeFilter, ref int summaryFolders, ref int summaryFiles)
    {
        FolderStatus status = OpenFolder(path);
        StringBuilder b = new StringBuilder();

        if (!summary)
        {
            string folderName =
                Global.Platform is Platform.Windows
                    ? Markup.SafeBackslash(Path.GetFileName(path)) + (currentLevel is 0 ? '\0' : @"\\")
                    : Path.GetFileName(path) + (path.Length is 1 && path is "/" or "~" ? '\0' : '/');

            b.Append(new string(' ', currentLevel * 2)); // Indent
            b.Append($"{folderIconUsable.color}"); // Color

            if (config.icons && !Console.IsOutputRedirected && folderName is not "/")
                b.Append(folderIconUsable.icon); // Icon

            if (string.IsNullOrEmpty(folderName)) folderName = path;

            b.Append(folderName); // folder name
            b.Append(status switch {
                FolderStatus.Ok => "",
                FolderStatus.Empty => " $[cyan](empty)$[white]",
                FolderStatus.AccessDenied => " $[red](access denied)$[white]",
                FolderStatus.NotFound => " $[yellow](not found)$[white]",
                _ => " $[red](error)$[white]"
            }); // status tag

            Markup.Rich($"**{b.ToString()}", true);
        }

        if (status != FolderStatus.Ok) return;

        // FILE
        if (typeFilter is 0 or 2)
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    if (currentLevel < maxLevels)
                    {
                        string fileName = Path.GetFileName(file);

                        if (config.IsIgnored(Path.GetExtension(fileName))) continue;

                        summaryFiles++;

                        if (!summary)
                        {
                            FileIcon(fileName, ref fileIconUsable);

                            b.Clear();
                            b.Append(new string(' ', (currentLevel + 1) * 2));
                            b.Append($"{fileIconUsable.color}");
                            b.Append(config.icons ? fileIconUsable.icon + ' ' : "");
                            b.Append(fileName);

                            Markup.Rich(b.ToString(), true);
                        }
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
                        summaryFolders++;
                        ExecuteTree(subDir, currentLevel + 1, maxLevels, summary, typeFilter, ref summaryFolders, ref summaryFiles);
                    }
                }
                catch { }
            }
        }
    }
    private void FileIcon(string fileName, ref FileIconPack fileIcon)
    {
        int end = fileName.Length - 1;
        while (end >= 0 && fileName[end] == ' ') end--;

        int dotIndex = -1;
        for (int i = end; i >= 0; i--)
        {
            if (fileName[i] == '.')
            {
                dotIndex = i;
                break;
            }
            if (fileName[i] == '/' || fileName[i] == '\\') break;
        }

        ReadOnlySpan<char> extSpan = dotIndex >= 0 
            ? fileName.AsSpan(dotIndex, end - dotIndex + 1) 
            : ReadOnlySpan<char>.Empty;

        if (Console.IsOutputRedirected)
        {
            fileIcon = new FileIconPack("","");
            return;
        }

        fileIcon = extSpan switch
        {
            // Language
            ".cs"      => new ("󰌛", "$[octavus]"),
            ".py"      => new ("", "$[platypus]"),
            ".js"      => new ("", "$[genesis]"),
            ".ts"      => new ("", "$[navy]"),
            ".cpp" or ".h" or ".hpp"
                       => new ("", "$[septima]"),
            ".c"       => new ("", "$[purplerain]"),
            ".go"      => new ("󰟓", "$[octavus]"),
            ".rs"      => new ("", "$[festive]"),
            ".java"    => new ("", "$[genesis]"),
            ".asm"     => new ("", "$[nonalux]"),

            // Mark & <>
            ".md" or ".markdown"
                       => new ("", "$[cyanish]"),
            ".html" or ".htm"
                       => new ("", "$[sextus]"),
            ".css"     => new ("", "$[mint]"),
            ".xaml"    => new ("󰙳", "$[sevenup]"),

            // Terminal Script & Configs & Data
            ".bat" or ".cmd"
                       => new ("", "$[catppuccin]"),
            ".ps1"     => new ("󰨊", "$[octavus]"),
            ".yaml" or ".yml"
                       => new ("", "$[septima]"),
            ".sql"     => new ("", "$[genesis]"),
            ".json"    => new ("", "$[barbie]"),
            ".jsonl"   => new ("󰘦", "$[barbie]"),
            ".txt"     => new ("󰦨", "$[catppuccin]"),
            ".pdf"     => new ("󰈦", "$[nona]"),
            ".xls" or ".xlsx"
                       => new ("", "$[cyanish]"),

            // Images & Graphics
            ".png"     => new ("󰸭", "$[octavus]"),
            ".jpg" or ".jpeg"
                       => new ("󰈥", "$[navy]"),
            ".webp"    => new ("", "$[octavus]"),
            ".svg"     => new ("󰜡", "$[platypus]"),
            ".gif"     => new ("󰵸", "$[white]"),
            ".ico"     => new ("", "$[nona]"),

            // Video & Audio
            ".mp4" or ".mkv" or ".avi" or ".mov"
                       => new ("", "$[octavus]"),
            ".mp3" or ".wav" or ".ogg"
                       => new ("", "$g[septima,barbie]"),

            // Archive & Disk Images
            ".zip" or ".rar" or ".tar" or ".gz" or ".7z"
                       => new ("󰿺", "$[mint]"),
            ".iso"     => new ("", "$[octavus]"),

            // System & Executables
            ".exe" or ".msi" or ".appimage"
                       => new ("󰣆", "$[magenta]"),
            ".dll"     => new ("", "$[catppuccin]"),
            ".desktop" => new ("", "$[cyanish]"),

            _ => new ("", "$[white]")
        };
    }
}