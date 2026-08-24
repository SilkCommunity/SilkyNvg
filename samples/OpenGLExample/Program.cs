using System.Numerics;
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
        _ctx = new SilkyRenderingContext2D(_renderer, 1280, 720);
        
        Resize(_window.Size);
    }

    private void Resize(Vector2D<int> newSize)
    {
        _gl!.Viewport(newSize);
        _ctx?.Resize((uint)newSize.X, (uint)newSize.Y);
    }

    private void Update(double delta)
    {
        
    }

    private void Render(double _)
    {
        _gl!.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

        var starPath = new Path2D();
        var circlePath1 = new Path2D();
        var circlePath2 = new Path2D();
        
        _ctx?.BeginFrame();

        starPath.MoveTo(250, 75);
        starPath.LineTo(323, 301);
        starPath.LineTo(131, 161);
        starPath.LineTo(369, 161);
        starPath.LineTo(177, 301);
        starPath.ClosePath();
        _ctx?.Fill(starPath);

        circlePath1.Arc(600, 188, 107, 0, MathF.Tau, counterclockwise: false);
        circlePath1.Arc(600, 188, 49, 0, MathF.Tau, counterclockwise: false);
        _ctx?.Fill(circlePath1);
        
        circlePath2.Arc(950, 188, 107, 0, MathF.Tau, counterclockwise: false);
        circlePath2.Arc(950, 188, 49, 0, MathF.Tau, counterclockwise: true);
        _ctx?.Fill(circlePath2);
        
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