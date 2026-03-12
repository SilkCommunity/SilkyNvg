// See https://aka.ms/new-console-template for more information

using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL;

namespace OpenGLExample;

internal class Program : IDisposable
{
    
    private readonly IWindow _window;

    private OpenGLRenderer _renderer;
    private SilkyRenderingContext _ctx;
    
    private GL _gl;

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

        _renderer = new OpenGLRenderer(_gl);
        _ctx = new SilkyRenderingContext(_renderer);
        
        Resize(_window.Size);
    }

    private void Resize(Vector2D<int> newSize)
    {
        _gl.Viewport(newSize);

        _ctx.Resize(newSize.X, newSize.Y, 1.0f);
    }

    private void Update(double delta)
    {
        
    }

    private void Render(double _)
    {
        _gl.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit);
    }

    private void Close()
    {
        _ctx.Dispose();
        _renderer.Dispose();
        _gl.Dispose();
    }

    public void Dispose()
    {
        
    }
    
    static void Main(string[] args)
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