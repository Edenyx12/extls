namespace extls.Tools;

public partial class Dir
{
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
}