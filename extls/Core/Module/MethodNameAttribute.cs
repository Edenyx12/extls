namespace extls.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MethodNameAttribute : Attribute
{
    public string[] Aliases { get; }

    public MethodNameAttribute(params string[] aliases)
    {
        this.Aliases = aliases;
    }
}