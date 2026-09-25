using System.Reflection;

namespace extls.Core;

public static partial class Global
{
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
                    methodAttribute.Aliases)
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

    public static ModuleMeta? GetModuleMeta(string name)
    {
        foreach (var key in Modules)
            for (int i = 0; i < key.Key.Length; i++)
                if (key.Key[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return Modules[key.Key];
        
        return null;
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

    public static bool ExecuteModule(Module module, ModuleMeta meta, string method)
    {
        if (module is null) return false;

        int methodIndex = -1;
        
        for (int i = 0; i < meta.Methods.Length; i++)
        {
            for (int j = 0; j < meta.Methods[i].Aliases.Length; j++)
            {
                if (method.Equals(meta.Methods[i].Aliases[j], StringComparison.OrdinalIgnoreCase))
                {
                    methodIndex = i;
                    break;
                }
            }
        }
        
        if (methodIndex == -1) return false;

        try
        {
            module.GetType()
              .GetMethod(meta.Methods[methodIndex].MethodName)?
              .Invoke(module, null);
        }
        catch
        {
            throw new Exception(
                $"Arguments detected in the signature of method `{meta.Methods[methodIndex].MethodName}`. " +
                "The method cannot have arguments when called automatically.");
        }
        
        return true;
    }
}