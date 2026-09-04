namespace extls.Core;

public enum Params
{
    None,
    Args,
    OneArg,
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MethodNameAttribute : Attribute
{
    public string[] Aliases { get; }
    public Params Params { get; }

    public MethodNameAttribute(Params @params = Params.None,params string[] aliases)
    {
        this.Aliases = aliases;
        this.Params = @params;
    }
}