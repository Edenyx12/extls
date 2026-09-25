using System.Diagnostics;
using extls.Core.Decoration;

namespace extls.Core.Modules;

public abstract class ModuleShell : Module
{
    /// <summary>
    /// Stopwatch to measure the time taken for the Shell to execute (only with verbose output).
    /// </summary>
    protected Stopwatch shellTime = new Stopwatch();
    /// <summary>
    /// If true, the Shell will not read input from the user. 
    /// This is useful for testing or when the Shell is being used in a non-interactive environment.
    /// </summary>
    protected bool lockread = false;
    /// <summary>
    /// Color type for the Shell prompt. 0 - white, 1 - red, 2 - green, 3 - yellow
    /// </summary>
    protected byte colorType = 0;

    /// <summary>
    /// Dispatches the command to the appropriate method based on the arguments provided.
    /// This method overrided from the base Module class to provide Shell-specific functionality.
    /// </summary>
    public override bool Dispatch(string arg)
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

        shellTime.Start();

        OnStart();
        ShowShell();
        OnExit();

        shellTime.Stop();

        return true;
    }

    /// <summary>
    /// This method is responsible for drawing the Shell to the console.
    /// It should be overridden in derived classes to provide custom Shell layouts.
    /// </summary>
    protected abstract void DrawShell();
    /// <summary>
    /// This method is called after the user has provided input.
    /// It should be overridden in derived classes to handle the input appropriately.
    /// </summary>
    protected abstract void AfterInput(string input);

    /// <summary>
    /// This method is responsible for displaying the Shell and handling user input.
    /// It will continue to display the Shell until the user exits or an error occurs.
    /// </summary>
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

    /// <summary>
    /// This method is called when the Shell starts.
    /// It can be overridden to provide custom behavior,
    /// such as displaying a welcome message or initializing resources.
    /// </summary>
    protected virtual void OnStart()
    {
        Markup.Rich($"$[green]{name}$[white] Shell - $[cyan]{version}$[white].\n" +
                    $"$[darkgray]`/h` for help, `/v` for version.$[white]", true);
    }
    /// <summary>
    /// This method is called when the Shell exits.
    /// It can be overridden to provide custom behavior,
    /// such as displaying a farewell message or cleaning up resources.
    /// </summary>
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