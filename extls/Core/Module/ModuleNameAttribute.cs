namespace extls.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleNameAttribute : Attribute
{
    public string[] Aliases { get; }

    public ModuleNameAttribute(params string[] aliases)
    {
        this.Aliases = aliases;
    }
}