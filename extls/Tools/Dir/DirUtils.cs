using extls.Core;

namespace extls.Tools;

public partial class Dir 
{
    private FileIconPack fileIconUsable = new FileIconPack("","");
    private void FileIcon(string fileName, ref FileIconPack fileIcon)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();;
        
        fileIcon.icon = extension switch
        {
            // Language
            ".cs" => "󰌛",
            ".py" => "",
            ".js" => "",
            ".ts" => "",
            ".cpp" or ".h" or ".hpp"=> "",
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
                => "",
            ".mp3" or ".wav" or ".ogg"
                => "",
            
            // Archive & Disk Images
            ".zip" or ".rar" or ".tar" or ".gz" or ".7z"
                => "󰿺",
            ".iso" => "",
            
            // System & Executables
            ".exe" or ".msi" or ".appimage"
                => "󰣆",
            ".dll" => "",
            
            _ => ""
        };
        fileIcon.color = extension switch
        {
            ".py" or "yaml" or ".yml" or ".xls" or ".xlsx" 
                  or ".png" or ".jpg" or ".jpeg" or ".webp" 
                  or ".svg" or ".gif" or ".ico" or ".ogg" 
                  or ".msi" or ".appimage"
                => "green",

            ".go" or ".cs" or ".css" 
                => "cyan",

            ".ts" or ".md" or ".markdown" 
                => "blue",

            ".java" or ".html" or ".htm" or ".pdf" 
                    or ".mp4" or ".mkv" or ".avi" or ".mov" 
                => "red",

            ".c" or ".cpp" or ".h" or ".hpp" or ".xaml" 
                 or ".sql" or ".json" or ".jsonl" 
                => "magenta",

            ".asm" or ".rs"
                => "darkred",

            _ => "white"
        };
    }
    private string Reason(Exception ex) { 
        return ex switch {
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

    private record struct FileIconPack(string icon, string color);
    private enum FolderStatus { Ok, Empty, AccessDenied, NotFound, Error }
}