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

        var path = new Path2D();

        float alpha0 = MathF.PI * 0.25f;
        path.Ellipse(600, 350, 500, 300, MathF.PI * 0.0f, MathF.PI * 0, -MathF.PI * 1.5f, true);
        path.ClosePath();
        //path.RoundRect(10, 20, 150, 100, 40);
        //path.RoundRect(10, 150, 150, 100, 10, 40);
        //path.RoundRect(400, 20, 200, 100, 0, 30, 50, 60);
        //path.RoundRect(400, 150, -200, 100, 0, 30, 50, 60);
        
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