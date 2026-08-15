using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL;

namespace OpenGLExample;

internal class Program : IDisposable
{
    
    private readonly IWindow _window;
    
    private GL? _gl;

    private SilkyOpenGLRenderer? _renderer;
    private SilkyRenderingContext2D? _ctx;
    
    private Program(IWindow window)
    {
        _window = window;

        _window.Load += Load;
        _window.Resize += Resize;
        _window.Update += Update;
        _window.Render += Render;
        _window.Closing += Close;
    }

    private void Load()
    {
        _gl = GL.GetApi(_window);

        _renderer = new SilkyOpenGLRenderer(_gl);
        _ctx = new SilkyRenderingContext2D(1280, 720, _renderer);
        
        Resize(_window.Size);
    }

    private void Resize(Vector2D<int> newSize)
    {
        
        _gl!.Viewport(newSize);
    }

    private void Update(double delta)
    {
        
    }

    private void Render(double _)
    {
        _gl!.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

        var path = new Path2D();
        /*path.MoveTo(250f, 75f);
        path.LineTo(323f, 301f);
        path.LineTo(131f, 161f);
        path.LineTo(369f, 161f);
        path.LineTo(177f, 301f);*/
        /*path.MoveTo(200.0f, 300.0f);
        path.QuadraticCurveTo(400.0f, 50.0f, 600.0f, 300.0f);
        path.QuadraticCurveTo(800.0f, 550.0f, 1000.0f, 300.0f);*/
        path.MoveTo(600.0f, 500.0f);
        path.BezierCurveTo(600f, 350f, 900f, 650f, 900f, 500f);
        path.ClosePath();

        _ctx?.BeginFrame();
        
        _ctx?.Fill(path);
        
        _ctx?.EndFrame();
    }

    private void Close()
    {
        _renderer?.Dispose();
        
        _gl!.Dispose();
    }

    public void Dispose()
    {
        
    }
    
    private static void Main()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(1280, 720);
        options.Title = "OpenGL Example";

        var window = Window.Create(options);
        var instance = new Program(window);

        window.Run();

        instance.Dispose();
        window.Dispose();
    }
    
}