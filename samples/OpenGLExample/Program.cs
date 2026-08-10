using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace OpenGLExample;

internal class Program : IDisposable
{
    
    private readonly IWindow _window;
    
    private GL? _gl;
    
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
        
        
    }

    private void Close()
    {
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