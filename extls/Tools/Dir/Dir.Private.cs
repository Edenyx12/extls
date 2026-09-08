using extls.Core;
using extls.Core.Decoration;

namespace extls.Tools;

public partial class Dir
{
    private FileIconPack fileIconUsable = new FileIconPack("", "");
    private void ExecuteTree(string path, int currentLevel, int maxLevels, bool summary, byte typeFilter, ref int sumDir, ref int sumF)
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
                FolderStatus.Empty => "[[cyan](empty)[white]]",
                FolderStatus.AccessDenied => "[[red](access denied)[white]]",
                FolderStatus.NotFound => "[[yellow](not found)[white]]",
                _ => "[[red](error)[white]]"
            };

            if (Root.Platform is Platform.Windows)
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
                    if (currentLevel < maxLevels) sumF++;
                    if (!summary && currentLevel < maxLevels)
                    {
                        string indent = new string(' ', (currentLevel + 1) * 2);
                        string fileName = System.IO.Path.GetFileName(file);
                        FileIcon(fileName, ref fileIconUsable);

                        Markup.Rich($"{indent}" +
                                    $"$[{fileIconUsable.color}]" +
                                    $"{(config.icons ? $"{fileIconUsable.icon} " : "")}" +
                                    $"{fileName}", true);
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
                        ExecuteTree(subDir, currentLevel + 1, maxLevels, summary, typeFilter, ref sumDir, ref sumF);
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

        fileIcon.icon = extSpan switch
        {
            // Language
            ".cs" => "󰌛",
            ".py" => "",
            ".js" => "",
            ".ts" => "",
            ".cpp" or ".h" or ".hpp" => "",
            ".c" => "",
            ".go" => "󰟓",
            ".rs" => "",
            ".java" => "",
            ".asm" => "",

            // Mark & <>
            ".md" or ".markdown" => "",
            ".html" or ".htm" => "",
            ".css" => "",
            ".xaml" => "󰙳",

            // Terminal Script & Configs & Data
            ".bat" or ".cmd" => "",
            ".ps1" => "󰨊",
            ".yaml" or ".yml" => "",
            ".sql" => "",
            ".json" => "",
            ".jsonl" => "󰘦",
            ".txt" => "󰦨",
            ".pdf" => "󰈦",
            ".xls" or ".xlsx" => "",

            // Images & Graphics
            ".png" => "󰸭",
            ".jpg" or ".jpeg" => "󰈥",
            ".webp" => "",
            ".svg" => "󰜡",
            ".gif" => "󰵸",
            ".ico" => "",

            // Video & Audio
            ".mp4" or ".mkv" or ".avi" or ".mov"
                => "",
            ".mp3" or ".wav" or ".ogg"
                => "",

            // Archive & Disk Images
            ".zip" or ".rar" or ".tar" or ".gz" or ".7z"
                => "󰿺",
            ".iso" => "",

            // System & Executables
            ".exe" or ".msi" or ".appimage"
                => "󰣆",
            ".dll" => "",
            ".desktop" => "",

            _ => ""
        };
        fileIcon.color = extSpan switch
        {
            ".py" or "yaml" or ".yml" or ".xls" or ".xlsx"
                  or ".png" or ".jpg" or ".jpeg" or ".webp"
                  or ".svg" or ".gif" or ".ico" or ".ogg"
                  or ".msi" or ".appimage"
                => "green",

            ".go" or ".cs" or ".css"
                => "red",

            ".ts" or ".md" or ".markdown" or ".desktop"
                => "blue",

            ".java" or ".html" or ".htm" or ".pdf"
                    or ".mp4" or ".mkv" or ".avi" or ".mov"
                => "cyan",

            ".c" or ".cpp" or ".h" or ".hpp" or ".xaml"
                 or ".sql" or ".json" or ".jsonl"
                => "magenta",

            ".asm" or ".rs"
                => "darkred",

            _ => "white"
        };
    }
    private record struct FileIconPack(string icon, string color);
    private readonly record struct GridItem(string name, bool isFolder);
}