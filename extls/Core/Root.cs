using System.Reflection;

namespace extls.Core;

public enum Platform
{
    Windows,
    Linux
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleNameAttribute : Attribute
{
    public string[] Names { get; }

    public ModuleNameAttribute(params string[] names)
    {
        Names = names;
    }
}

public static class Root
{
    public static Assembly Assembly = Assembly.GetExecutingAssembly();
    public static Platform Platform = Platform.Windows;
    public static string Version = "0.3-alpha";
    public static readonly string RootPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".extls"
    );

    private static Dictionary<string[], string>? _modules { get; set; }
    public static Dictionary<string[], string> Modules
    {
        get
        {
            if (_modules is null)
                _modules = JsonService.LoadJson<Dictionary<string[], string>>(
                    RootPath, "modules.json"
                );

            if (_modules is null)
                _modules = GenerateReflection();

            return _modules;
        }
        set
        {
            _modules = value;
            JsonService.SaveJson(RootPath, "modules.json", _modules);
        }
    }

    static Root()
    {
        if (OperatingSystem.IsWindows()) Platform =  Platform.Windows;
        else if (OperatingSystem.IsLinux()) Platform =  Platform.Linux;
    }
    
    static Dictionary<string[], string> GenerateReflection()
    {
        var result = new Dictionary<string[], string>();

        foreach (Type type in Assembly.GetTypes())
        {
            if (!type.IsClass ||
                type.IsAbstract ||
                !typeof(Module).IsAssignableFrom(type))
                continue;

            var attribute = type.GetCustomAttribute<ModuleNameAttribute>();

            if (attribute is null)
                continue;

            result.Add(attribute.Names, type.FullName!);
        }

        return result;
    }
    
    public static Module? GetModule(string name)
    {
        string moduleName = string.Empty;
        
        foreach (var key in Modules)
        {
            for (int i = 0; i < key.Key.Length; i++)
            {
                if (key.Key[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    moduleName = Modules[key.Key];
                    break;
                }
            }

            if (moduleName != string.Empty) break;
        }
        if (moduleName == string.Empty) return null!;
        
        var module = Assembly.GetType(moduleName);
        
        if (module is null) return null!;
        return Activator.CreateInstance(module) as Module;
    }
}