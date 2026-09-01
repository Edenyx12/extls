namespace extls.Core;

public enum Platform
{
    Windows,
    Linux
}

public static class Root
{
    public static Platform Platform = Platform.Windows;
    public static string Version = "0.2.34-alpha";

    static Root()
    {
        if (OperatingSystem.IsWindows()) Platform =  Platform.Windows;
        else if (OperatingSystem.IsLinux()) Platform =  Platform.Linux;
    }
}