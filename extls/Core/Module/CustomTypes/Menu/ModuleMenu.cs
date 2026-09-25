using System.Text;
using extls.Core;
using extls.Core.Decoration;
using extls.Core.Modules.Menu;

namespace extls.Core.Modules;

public abstract class ModuleMenu : ModuleRaw
{
    public static MTTheme theme = MTTheme.Dark;

    protected List<MMObj> objs = new();
    protected bool running = true;
    protected int framerate = 60;
    protected int width = Console.WindowWidth;
    protected int height = Console.WindowHeight;

    protected event Action<ConsoleKey>? OnInput;

    private StringBuilder pen = new();
    private MTAtlas atlas = new();

    private readonly object _renderLock = new object();
    private Color _lastFg;
    private Color _lastBg;

    public MM? AddWithGet<MM>(MMObj obj) where MM : MMObj { objs.Add(obj); return obj as MM; }

    public override bool DispatchRaw(string arg)
    {
        if (base.DispatchRaw2(arg)) return false;

        OnInput += (key) => { if (key is ConsoleKey.Escape) running = false; };

        Run().GetAwaiter().GetResult();

        return true;
    }

    protected virtual void Draw()
    {
        lock (_renderLock)
        {
            pen.Clear();

            width = Console.WindowWidth;
            height = Console.WindowHeight;
            
            atlas.size = new MTTwo(width,height);
            atlas.Refresh();

            foreach (MMObj obj in objs)
            {
                obj.Compose(ref atlas);
            }

            for (int y = 0; y < atlas.size.two; y++)
            {
                for (int x = 0; x < atlas.size.one; x++)
                {
                    if (!_lastFg.Equals(atlas.atlas[y, x].fg))
                    {
                        _lastFg = atlas.atlas[y, x].fg;
                        pen.Append(Color.ColorToConsoleFg(_lastFg));
                    }
                    if (!_lastBg.Equals(atlas.atlas[y, x].bg))
                    {
                        _lastBg = atlas.atlas[y, x].bg;
                        if (!(theme is MTTheme.Dark && _lastBg.Equals(MTCell.GetBgByTheme())))
                            pen.Append(Color.ColorToConsoleBg(_lastBg));
                        else Console.ResetColor();
                    }

                    pen.Append(atlas.atlas[y,x].c);
                }

                if (y < atlas.size.two - 1)
                    pen.AppendLine();
            }

            Console.SetCursorPosition(0, 0);
            Out.Inline(pen.ToString());
        }
    }

    protected virtual async Task Run()
    {
        Console.ResetColor();
        Console.CursorVisible = false;
        Out.EnableAlternateBuffer();

        running = true;
        int msDelay = 1000 / framerate;

        // Render
        Task renderTask = Task.Run(async () =>
        {
            while (true)
            {
                lock (_renderLock)
                {
                    if (!running) break;
                }

                Draw();
                await Task.Delay(msDelay);
            }
        });

        // Input
        Task inputTask = Task.Run(() =>
        {
            while (true)
            {
                lock (_renderLock)
                {
                    if (!running) break;
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true).Key;
                    OnInput?.Invoke(key);
                }
                else Thread.Sleep(5);
            }
        });

        await Task.WhenAll(renderTask, inputTask);

        Out.DisableAlternateBuffer();
        Console.CursorVisible = true;
    }

}