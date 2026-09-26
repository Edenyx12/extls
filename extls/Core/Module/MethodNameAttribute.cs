namespace extls.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MethodNameAttribute : Attribute
{
    public string Description { get; }
    public string[] Aliases { get; }

    public MethodNameAttribute(
        string[] aliases,
        string description = "")
    {
        this.Description = description;
        this.Aliases = aliases;
    }
}
