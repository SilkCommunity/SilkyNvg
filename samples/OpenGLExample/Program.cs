using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Renderers.OpenGL;
using SilkyNvg.Rendering;
using Path = SilkyNvg.Paths.Path;

internal class Programme : IDisposable
{

    private readonly IWindow _window;

    private GL _gl;
    
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
        _gl = GL.GetApi(_window);
        
        _renderer = new OpenGLRenderer(_gl);

        var settings = new SilkyRenderingContextSettings
        (
            Alpha: true,
            Desynchronised: false
        );
    
        _ctx = new SilkyRenderingContext(settings, _renderer);
        Resize(_window.Size);
    }

    private void Resize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _ctx!.Resize(size.X, size.Y);
    }

    private void Update(double delta)
    {
    
    }

    private void Render(double _)
    {
        _gl.ClearColor(0.0f, 0.3f, 1.0f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        var path = new Path();
        path.MoveTo(250.0f, 75.0f);
        path.LineTo(500.0f, 75.0f);

        _renderer.Render();
    }

    private void Closing()
    {
        _ctx!.Dispose();
        _renderer.Dispose();
        _gl.Dispose();
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