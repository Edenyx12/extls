using extls.Core;
using extls.Core.Decoration;
using extls.Core.Modules;
using extls.Core.Modules.Menu;

namespace extls;

class MM : ModuleMenu
{
    public MM()
    {
        var layout = AddWithGet<MMLayout>(new MMLayout(AlignH.Stretch, AlignV.Stretch));
        layout!.align = AlignLayout.Horizontal;

        for (int x = 0; x < 5; x++)
        {
            MMLayout xl = new(new MTTwo(1,1), new MTTwo(0,0));
            xl.alignv = AlignV.Stretch;
            xl.align = AlignLayout.Vertical;
            layout!.Append(xl);
            
            for (int y = 0; y < 3; y++)
            {
                var yf = new MMFrame(new MTTwo(1,1), new MTTwo(0,0));
                yf.alignh = AlignH.Stretch;
                xl.Append(yf);
            }
        }

        /*var txt = AddWithGet<MMText>(new MMText(AlignH.Absolute, AlignV.Absolute));
        txt!.pos = new MTTwo(1,1);
        txt!.size = new MTTwo(4,30);
        txt.elide = true;
        txt.text = "Test test test text test text this is text test text \n\n jopa jopa jopa";*/
        
        OnInput += (key) =>
        {
            
        };
    }
}

public class Program
{
    static void Main(string[] args)
    {
        /*var mm = new MM();

        mm.DispatchRaw(args);

        return;*/

        var root = new Root();

        if (args.Length == 0)
        {
            root.Label();
            return;
        }

        string moduleName = args[0].ToLower();;
        string[] cleanArgs = args[1..];

        Arguments.Initialize(cleanArgs);

        if (moduleName is "root")
        {
            root.Dispatch();
            return;
        }
        else if (moduleName is "v" or "--version" or "version")
        {
            root.Version();
            return;
        }

        var module = Global.GetModule(moduleName);
        
        if (module != null)
        {
            module.Dispatch(cleanArgs.Length > 0 ? cleanArgs[0] : "");
            return;
        }
        
        Out.Warning($"Module not found: '{moduleName}'. Check modules with `extls modules`.");
    }
}