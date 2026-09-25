using extls.Core;
using extls.Core.Decoration;

namespace extls.Tools;

public partial class Dir
{
    private FileIconPack fileIconUsable = new FileIconPack("", "");
    private void ExecuteTree(string path, int currentLevel, int maxLevels, bool summary, byte typeFilter, ref int summaryFolders, ref int summaryFiles)
    {
        FolderStatus status = OpenFolder(path);

        if (!summary)
        {
            string indent = new string(' ', currentLevel * 2);
            string folderName = System.IO.Path.GetFileName(path);
            string folderIcon = config.icons ? " " : "";

            if (string.IsNullOrEmpty(folderName)) folderName = path;

            string statusTag = status switch
            {
                FolderStatus.Ok => "",
                FolderStatus.Empty => "$[cyan](empty)$[white]",
                FolderStatus.AccessDenied => "$[red](access denied)$[white]",
                FolderStatus.NotFound => "$[yellow](not found)$[white]",
                _ => "$[red](error)$[white]"
            };

            if (Global.Platform is Platform.Windows)
                Markup.Rich($"{indent}$[yellow]{folderIcon}{folderName}\\\\ {statusTag}", true);
            else
            {
                if (folderName is "/")
                    Markup.Rich($"{indent}$[#7dbeff]{folderName} {statusTag}", true);
                else Markup.Rich($"{indent}$[#7dbeff]{folderIcon}{folderName}/ {statusTag}", true);
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
                    if (currentLevel < maxLevels)
                    {
                        string fileName = Path.GetFileName(file);

                        if (config.IsIgnored(Path.GetExtension(fileName))) continue;

                        summaryFiles++;

                        if (!summary)
                        {
                            string indent = new string(' ', (currentLevel + 1) * 2);

                            FileIcon(fileName, ref fileIconUsable);

                            Markup.Rich($"{indent}" +
                                        $"$[{fileIconUsable.color}]" +
                                        $"{(config.icons ? $"{fileIconUsable.icon} " : "")}" +
                                        $"{fileName}", true);
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
    private static FolderStatus OpenFolder(string path)
    {
        try
        {
            using var en = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
            if (!en.MoveNext()) return FolderStatus.Empty;
        }
        catch (UnauthorizedAccessException) { return FolderStatus.AccessDenied; }
        catch (DirectoryNotFoundException) { return FolderStatus.NotFound; }
        catch (Exception) { return FolderStatus.Error; }
        return FolderStatus.Ok;
    }
    private string Reason(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => "Access Denied",
            DirectoryNotFoundException => "Directory Not Found",
            PathTooLongException => "Path Too Long",
            ArgumentNullException => "Path Is Null",
            ArgumentException => "Invalid Path Arguments",
            NotSupportedException => "Path Format Not Supported",
            IOException => "I/O Error",
            _ => "Unknown Error"
        };
    }
    private enum FolderStatus { Ok, Empty, AccessDenied, NotFound, Error }
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

        fileIcon = extSpan switch
        {
            // Language
            ".cs"      => new ("󰌛", "octavus"),
            ".py"      => new ("", "platypus"),
            ".js"      => new ("", "genesis"),
            ".ts"      => new ("", "navy"),
            ".cpp" or ".h" or ".hpp"
                       => new ("", "septima"),
            ".c"       => new ("", "purplerain"),
            ".go"      => new ("󰟓", "octavus"),
            ".rs"      => new ("", "festive"),
            ".java"    => new ("", "genesis"),
            ".asm"     => new ("", "nonalux"),

            // Mark & <>
            ".md" or ".markdown"
                       => new ("", "cyanish"),
            ".html" or ".htm"
                       => new ("", "sextus"),
            ".css"     => new ("", "mint"),
            ".xaml"    => new ("󰙳", "sevenup"),

            // Terminal Script & Configs & Data
            ".bat" or ".cmd"
                       => new ("", "catppuccin"),
            ".ps1"     => new ("󰨊", "octavus"),
            ".yaml" or ".yml"
                       => new ("", "septima"),
            ".sql"     => new ("", "genesis"),
            ".json"    => new ("", "barbie"),
            ".jsonl"   => new ("󰘦", "barbie"),
            ".txt"     => new ("󰦨", "catppuccin"),
            ".pdf"     => new ("󰈦", "nona"),
            ".xls" or ".xlsx"
                       => new ("", "cyanish"),

            // Images & Graphics
            ".png"     => new ("󰸭", "octavus"),
            ".jpg" or ".jpeg"
                       => new ("󰈥", "navy"),
            ".webp"    => new ("", "octavus"),
            ".svg"     => new ("󰜡", "platypus"),
            ".gif"     => new ("󰵸", "white"),
            ".ico"     => new ("", "nona"),

            // Video & Audio
            ".mp4" or ".mkv" or ".avi" or ".mov"
                       => new ("", "octavus"),
            ".mp3" or ".wav" or ".ogg"
                       => new ("", "septima"),

            // Archive & Disk Images
            ".zip" or ".rar" or ".tar" or ".gz" or ".7z"
                       => new ("󰿺", "mint"),
            ".iso"     => new ("", "octavus"),

            // System & Executables
            ".exe" or ".msi" or ".appimage"
                       => new ("󰣆", "magenta"),
            ".dll"     => new ("", "catppuccin"),
            ".desktop" => new ("", "cyanish"),

            _ => new ("", "cyan")
        };
    }
    private record struct FileIconPack(string icon, string color);
    private readonly record struct GridItem(string name, bool isFolder);
}