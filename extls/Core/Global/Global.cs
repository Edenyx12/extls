using System.Reflection;

namespace extls.Core;

public enum Platform
{
    Windows,
    Linux
}

public static partial class Global
{
    public static Assembly Asm = Assembly.GetExecutingAssembly();
    public static Platform Platform = Platform.Windows;
    public static readonly string Version =
        Asm
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion
            .Split('+')[0]
        ?? "unknown";
    public static bool Verbose = false;
    public static readonly string RootPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".edx",
        "extls"
    );
    public static readonly string HomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    static Global()
    {
        if (OperatingSystem.IsWindows()) Platform =  Platform.Windows;
        else if (OperatingSystem.IsLinux()) Platform =  Platform.Linux;
    }
}