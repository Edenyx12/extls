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
    public static string Version = "0.3.1-alpha";
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
            {
                ModuleReflectEntry[] mrentry = JsonService.LoadJson<ModuleReflectEntry[]>(
                    RootPath, "modules.json"
                );
                
                _modules = ModuleReflectEntry.GenerateFromEntry(mrentry);
            }

            if (_modules is null)
                Modules = GenerateReflection();

            return _modules!;
        }
        private set
        {
            _modules = value;
            
            ModuleReflectEntry[]? mrentry = ModuleReflectEntry.GenerateFromDictionary(_modules);
            JsonService.SaveJson(RootPath, "modules.json", mrentry!);
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

public struct ModuleReflectEntry
{
    public string[] aliases { get; set; }
    public string typeName { get; set; }
    
    public ModuleReflectEntry(string[] aliases, string typeName)
    {
        this.aliases = aliases;
        this.typeName = typeName;
    }

    public static ModuleReflectEntry[]? GenerateFromDictionary(Dictionary<string[], string>? dict)
    {
        if (dict is null || dict.Count == 0) return null!;
        
        ModuleReflectEntry[] result = new ModuleReflectEntry[dict.Count];
        
        int i = 0;
        foreach (var keyv in dict)
        {
            result[i] = new ModuleReflectEntry(keyv.Key, keyv.Value);
            i++;
        }
        
        return result;
    }

    public static Dictionary<string[], string>? GenerateFromEntry(ModuleReflectEntry[]? entries)
    {
        if (entries is null || entries.Length == 0) return null!;
        
        Dictionary<string[], string> result = new Dictionary<string[], string>();

        for (int i = 0; i < entries.Length; i++)
            result.Add(entries[i].aliases, entries[i].typeName);
        
        return result;
    }
}