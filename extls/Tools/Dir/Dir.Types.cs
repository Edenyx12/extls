namespace extls.Tools;

public partial class Dir
{
    private FileIconPack folderIconUsable = new FileIconPack(" ", "$[#89B4FA]"); // #89B4FA
    private FileIconPack fileIconUsable = new FileIconPack("", "");

    private record struct FileIconPack(string icon, string color);
    private readonly record struct GridItem(string name, bool isFolder);
    private enum FolderStatus { Ok, Empty, AccessDenied, NotFound, Error }
}