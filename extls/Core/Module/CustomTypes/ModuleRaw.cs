namespace extls.Core.Modules;

public abstract class ModuleRaw : Module
{
    public override bool Dispatch(string arg) => DispatchRaw(arg);
    
    public virtual bool DispatchRaw(string arg)
    {
        if (arg is "-v" or "--version")
        {
            Version();
            return true;
        }

        if (arg is "help" or "-h" or "--help")
        {
            Help();
            return true;
        }

        return false;
    }
    public virtual bool DispatchRaw2(string arg)
    {
        if (arg == "") return false;

        if (arg is "-v" or "--version")
        {
            Version();
            return true;
        }

        if (arg is "help" or "-h" or "--help")
        {
            Help();
            return true;
        }

        return false;
    }
}