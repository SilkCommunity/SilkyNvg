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
        _ctx = new SilkyRenderingContext2D(_renderer);
        
        Resize(_window.Size);
    }

    private void Resize(Vector2D<int> newSize)
    {
        _ctx!.Resize(newSize.X, newSize.Y, 1.0f);
        _gl!.Viewport(newSize);
    }

    private void Update(double delta)
    {
        
    }

    private void Render(double _)
    {
        _gl!.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);
        
        _ctx!.BeginFrame();

        var path = new Path2D();
        path.MoveTo(100.0f, 500.0f);
        path.QuadraticCurveTo(250.0f, 425.0f, 400.0f, 500.0f);
        path.QuadraticCurveTo(250.0f, 575.0f, 100.0f, 500.0f);
        path.ClosePath();
        path.MoveTo(200.0f, 500.0f);
        path.QuadraticCurveTo(250.0f, 450.0f, 300.0f, 500.0f);
        path.QuadraticCurveTo(250.0f, 550.0f, 200.0f, 500.0f);
        path.ClosePath();
        /*path.LineTo(300.0f, 500.0f);
        path.ClosePath();*/
        
        /*path.MoveTo(250, 75);
        path.LineTo(323, 301);
        path.LineTo(131, 161);
        path.LineTo(369, 161);
        path.LineTo(177, 301);
        path.ClosePath();*/
        /*path.MoveTo(320, 525);
        path.LineTo(445, 1025);
        path.LineTo(770, 700);
        path.LineTo(1095, 975);
        path.LineTo(1120, 500);
        path.LineTo(620, 200);
        path.LineTo(520, 400);
        path.LineTo(320, 525);
        path.ClosePath();*/
        
        _ctx.Fill(path);
        
        _ctx.EndFrame();
    }

    private void Close()
    {
        _ctx!.Dispose();
        _renderer!.Dispose();
        
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