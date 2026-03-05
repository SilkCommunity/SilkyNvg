using Silk.NET.Maths;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering;

internal class Programme : IDisposable
{

    private readonly IWindow _window;

    private ISilkyRenderer _renderer;
    private SilkyRenderingContext? _ctx;
    
    private Programme(IWindow window)
    {
        _window = window;
        _window.Load += Load;
        _window.Update += Update;
        _window.Render += Render;
        _window.Resize += Resize;
        _window.Closing += Closing;
    }
    
    private void Load()
    {
        _renderer = null;

        var settings = new SilkyRenderingContextSettings()
        {
            Alpha = true,
            Desynchronised = false
        };
    
        _ctx = new SilkyRenderingContext(settings, _renderer);
        _ctx.Resize(_window.Size.X, _window.Size.Y);
    }

    private void Resize(Vector2D<int> size)
    {
        _ctx!.Resize(size.X, size.Y);
    }

    private void Update(double delta)
    {
    
    }

    private void Render(double _)
    {
    
    }

    private void Closing()
    {
    
    }

    public void Dispose()
    {
        _window.Dispose();
    }
    
    private static void Main()
    {
        var options = WindowOptions.Default;
        options.Title = "OpenGL Example";
        options.Size = new Vector2D<int>(1280, 720);
        var window = Window.Create(options);
        
        var programme = new Programme(window);
        window.Run();

        programme.Dispose();
    }
    
}