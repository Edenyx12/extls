namespace extls.Core;

public struct MethodMeta
{
    public string MethodName { get; set; }
    public string[] Aliases { get; set; }
    public Params Params { get; set; }

    public MethodMeta(string methodName, string[] aliases, Params @params = Params.None)
    {
        this.MethodName = methodName;
        this.Aliases = aliases;
        this.Params = @params;
    }
}
public class ModuleMeta
{
    public string TypeName { get; set; }
    public MethodMeta[] Methods { get; set; }
    public string[] Aliases { get; set; }

    public ModuleMeta(string typeName, MethodMeta[] methods, string[] aliases)
    {
        this.TypeName = typeName;
        this.Methods = methods;
        this.Aliases = aliases;
    }

    public static Dictionary<string[], ModuleMeta> ConvertToDictionary(ModuleMeta[] jsons)
    {
        if (jsons is null || jsons.Length is 0) return null!;

        Dictionary<string[], ModuleMeta> dict = new();
        for (int i = 0; i < jsons.Length; i++)
            dict.Add(jsons[i].Aliases, jsons[i]);

        return dict;
    }

    public static ModuleMeta[] ConvertToJsons(Dictionary<string[], ModuleMeta> dict)
    {
        if (dict is null || dict.Count is 0) return null!;

        ModuleMeta[] jsons = new ModuleMeta[dict.Count];
        int i = 0;

        foreach (var t in dict)
        {
            jsons[i] = t.Value;
            i++;
        }

        return jsons;
    }
}