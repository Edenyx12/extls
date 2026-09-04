using System.Reflection;

namespace extls.Core;

public enum Platform
{
    Windows,
    Linux
}

public static class Root
{
    public static Assembly Assembly = Assembly.GetExecutingAssembly();
    public static Platform Platform = Platform.Windows;
    public static string Version = "0.4-alpha";
    public static readonly string RootPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".extls"
    );
    
    private static Dictionary<string[], ModuleMeta>? _modules;
    public static Dictionary<string[], ModuleMeta> Modules
    {
        get
        {
            if (_modules is null)
            {
                ModuleMeta[] json = JsonService.LoadJson<ModuleMeta[]>(
                    RootPath, "modules.json"
                );

                _modules = ModuleMeta.ConvertToDictionary(json);
            }
            
            if (_modules is null)
            {
                _modules = GenerateReflectionCache();

                ModuleMeta[] json = ModuleMeta.ConvertToJsons(_modules);
                JsonService.SaveJson(RootPath, "modules.json", json);
            }

            return _modules!;
        }
        private set
        {
            _modules = value;

            ModuleMeta[] json = ModuleMeta.ConvertToJsons(_modules);
            JsonService.SaveJson(RootPath, "modules.json", json);
        }
    }

    static Root()
    {
        if (OperatingSystem.IsWindows()) Platform =  Platform.Windows;
        else if (OperatingSystem.IsLinux()) Platform =  Platform.Linux;
    }
    
    static Dictionary<string[], ModuleMeta> GenerateReflectionCache()
    {
        var result = new Dictionary<string[], ModuleMeta>();

        foreach (Type type in Assembly.GetTypes())
        {
            if (!type.IsClass ||
                type.IsAbstract ||
                !typeof(Module).IsAssignableFrom(type))
                continue;

            var attribute = type.GetCustomAttribute<ModuleNameAttribute>();

            if (attribute is null)
                continue;

            var instance = Activator.CreateInstance(type) as Module;

            List<MethodMeta> methods = new List<MethodMeta>();

            foreach (var method in instance?.GetType()
                                        .GetMethods(BindingFlags.Public | BindingFlags.Instance) 
                                        ?? Array.Empty<MethodInfo>())
            {
                if (method.DeclaringType != type) continue;

                var methodAttribute = method.GetCustomAttribute<MethodNameAttribute>();
                if (methodAttribute is null) continue;

                methods.Add(new MethodMeta(
                    method.Name,
                    methodAttribute.Aliases,
                    methodAttribute.Params)
                );
            }

            List<string> aliases = new List<string>(attribute.Aliases);
            string moduleName = instance?.GetType().GetField("name")?.GetValue(instance)?.ToString() ?? string.Empty;
            if (moduleName != string.Empty && !aliases.Contains(moduleName))
                aliases.Add(moduleName);

            result.Add(attribute.Aliases, new ModuleMeta (
                instance?.GetType().FullName!,
                methods.ToArray(),
                aliases.ToArray()
            ));
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
                    moduleName = Modules[key.Key].TypeName;
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

    public static ModuleMeta GetModuleMeta(string name)
    {
        foreach (var key in Modules)
            for (int i = 0; i < key.Key.Length; i++)
                if (key.Key[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return Modules[key.Key];
        
        return null!;
    }

    public static bool ExecuteModule(Module module, ModuleMeta meta, string[] args)
    {
        if (module is null) return false;

        int methodIndex = -1;

        for (int i = 0; i < meta.Methods.Length; i++)
        {
            for (int j = 0; j < meta.Methods[i].Aliases.Length; j++)
            {
                if (args.Length > 0 && args[0].Equals(meta.Methods[i].Aliases[j], StringComparison.OrdinalIgnoreCase))
                {
                    methodIndex = i;
                    break;
                }
            }
        }
        Console.WriteLine(methodIndex.ToString());
        if (methodIndex == -1) return false;

        switch (meta.Methods[methodIndex].Params)
        {
            case Params.None:
                module.GetType()
                    .GetMethod(meta.Methods[methodIndex].MethodName)?
                    .Invoke(module, null);
                break;
            case Params.Args:
                string[] cleanArgs = Utils.RemoveZeroCommand(args);
                module.GetType()
                    .GetMethod(meta.Methods[methodIndex].MethodName)?
                    .Invoke(module, new object[] { cleanArgs }); 
                break;
            case Params.OneArg:
                module.GetType()
                    .GetMethod(meta.Methods[methodIndex].MethodName)?
                    .Invoke(module, new object[] { args[0] });
                break;
        }
        
        return true;
    }
}