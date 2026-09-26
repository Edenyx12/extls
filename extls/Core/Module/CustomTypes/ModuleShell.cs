using System.Diagnostics;
using extls.Core.Decoration;

namespace extls.Core.Modules;

public abstract class ModuleShell : Module
{
    protected Stopwatch shellTime = new Stopwatch();
    protected bool lockread = false;
    protected byte colorType = 0;

    public override bool Dispatch(string arg)
    {
        if (arg is "-v" or "--version")
        {
            Version();
            return true;
        }

        if (arg is "-h" or "--help")
        {
            Help();
            return true;
        }

        shellTime.Start();

        OnStart();
        ShowShell();
        OnExit();

        shellTime.Stop();

        return true;
    }

    protected abstract void DrawShell();
    protected abstract void AfterInput(string input);

    protected virtual void ShowShell()
    {
        while (true)
        {
            DrawShell();

            string color = colorType switch {
                1 => "$[red]>$[white]",
                2 => "$[green]>$[white]",
                3 => "$[yellow]>$[white]",
                _ => "$[white]>$[white]"
            };
            
            Markup.Rich($"\n$[cyan]{name}$[white] Shell {color} ", false);
            colorType = 0;
            string? input = Console.ReadLine();
            Console.WriteLine();
            
            if (input == string.Empty)
            {
                colorType = 3;
                continue;
            }

            if (!lockread)
            {
                switch (input!.ToLower())
                {
                    case "/h" or "/help":
                        colorType = 2;
                        Help();
                        continue;
                    case "/v" or "/version":
                        colorType = 2;
                        Version();
                        Console.WriteLine();
                        continue;
                    case "/q" or "/quit":
                        return;
                }
            }

            AfterInput(input!);
        }
    }

    protected virtual void OnStart()
    {
        Markup.Rich($"$[green]{name}$[white] Shell - $[cyan]{version}$[white].\n" +
                    $"$[darkgray]`/h` for help, `/v` for version.$[white]", true);
    }
    protected virtual void OnExit()
    {
        Markup.Rich($"$[gray]Exiting *{name}* Shell.", true);

        if (Global.Verbose)
        {
            TimeSpan elapsed = shellTime.Elapsed;

            string time = elapsed.TotalSeconds >= 1
                ? $"{elapsed.TotalSeconds:F2}s"
                : $"{elapsed.TotalMilliseconds:F2}ms";

            Markup.Rich($"$[darkgray]Shell execution time: $[yellow]{time}", true);
        }
    }
}