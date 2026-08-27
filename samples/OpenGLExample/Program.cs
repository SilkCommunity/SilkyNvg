using System.Numerics;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL;

namespace OpenGLExample;

internal class Program : IDisposable
{
    
    private static void OpenGlDebugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
    {
        if (type == GLEnum.DebugTypeError)
        {
            string msg = SilkMarshal.PtrToString(message) ?? "(unavailable)";
            Console.Error.WriteLine($"OpenGL Error: {source} {type} {id} {severity} \"{msg}\"");
        }
    }
    
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

    private unsafe void Load()
    {
        _gl = GL.GetApi(_window);

        // Setup GL debug callback
        _gl.DebugMessageCallback(OpenGlDebugCallback, null);
        
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
        _gl!.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);
        _gl.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);

        var path = new Path2D();
        
        _ctx?.BeginFrame();

        path.MoveTo(250, 75);
        path.LineTo(323, 301);
        path.LineTo(131, 161);
        path.LineTo(369, 161);
        path.LineTo(177, 301);
        path.ClosePath();
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