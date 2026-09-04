namespace extls.Core.Modules;

public abstract class ModuleRaw : Module
{
    public override bool Dispatch(string[] args) => DispatchRaw(args);
    
    public virtual bool DispatchRaw(string[] args)
    {
        if (args.Length <= 0 || (args[0] is "-v" or "--version"))
        {
            Version();
            return true;
        }

        if (args[0] is "help" or "-h" or "--help")
        {
            Help();
            return true;
        }

        return false;
    }
}