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
        
        _gl!.Viewport(0, 0, 1340, 1800);
    }

    private void Update(double delta)
    {
        
    }

    private void Render(double _)
    {
        _gl!.ClearColor(0.2f, 0.3f, 0.4f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

        var path = new Path2D();

        path.MoveTo(562, -21);
        path.BezierCurveTo(562, 68, 497, 129, 407, 129);
        path.BezierCurveTo(414, 85, 441, -74, 462, -194);
        path.BezierCurveTo(533, -165, 562, -92, 562, -21);
        path.ClosePath();
        
        path.MoveTo(420, -206);
        
        path.MoveTo(362, 123);
        path.BezierCurveTo(303, 109, 258, 60, 258, -1);
        path.BezierCurveTo(258, -50, 280, -76, 319, -100);
        path.BezierCurveTo(331, -108, 341, -113, 341, -122);
        path.BezierCurveTo(341, -131, 332, -135, 324, -135);
        path.BezierCurveTo(244, -135, 189, -39, 189, 31);
        path.BezierCurveTo(189, 125, 251, 221, 342, 248);
        path.BezierCurveTo(335, 289, 328, 336, 319, 390);
        path.BezierCurveTo(304, 375, 288, 361, 271, 346);
        path.BezierCurveTo(183, 270, 97, 161, 97, 39);
        path.BezierCurveTo(97, -112, 219, -212, 362, -212);
        path.BezierCurveTo(381, -212, 400, -210, 420, -206);
        path.ClosePath();
        
        path.MoveTo(332, 822);
        path.BezierCurveTo(324, 791, 321, 757, 321, 720);
        path.BezierCurveTo(321, 678, 326, 639, 332, 599);
        path.BezierCurveTo(401, 667, 478, 745, 478, 849);
        path.BezierCurveTo(478, 918, 454, 967, 439, 967);
        path.BezierCurveTo(387, 967, 341, 862, 332, 822);
        path.ClosePath();
        
        path.MoveTo(122, -513);
        path.BezierCurveTo(122, -447, 167, -390, 237, -390);
        path.BezierCurveTo(312, -390, 353, -447, 353, -501);
        path.BezierCurveTo(353, -565, 306, -605, 259, -612);
        path.BezierCurveTo(256, -613, 254, -614, 254, -616);
        path.BezierCurveTo(254, -617, 256, -618, 257, -619);
        path.BezierCurveTo(259, -619, 280, -624, 304, -624);
        path.BezierCurveTo(405, -624, 458, -569, 458, -465);
        path.BezierCurveTo(458, -412, 447, -342, 428, -246);
        path.BezierCurveTo(405, -250, 378, -253, 349, -253);
        path.BezierCurveTo(163, -253, 0, -106, 0, 81);
        path.BezierCurveTo(0, 281, 126, 402, 217, 487);
        path.BezierCurveTo(238, 504, 290, 557, 291, 558);
        path.BezierCurveTo(274, 670, 269, 719, 269, 773);
        path.BezierCurveTo(269, 857, 287, 985, 351, 1061);
        path.BezierCurveTo(384, 1100, 415, 1112, 422, 1112);
        path.BezierCurveTo(440, 1112, 469, 1077, 493, 1026);
        path.BezierCurveTo(509, 990, 537, 916, 537, 825);
        path.BezierCurveTo(537, 666, 464, 541, 358, 420);
        path.BezierCurveTo(367, 374, 377, 315, 387, 255);
        path.BezierCurveTo(533, 255, 640, 153, 640, 2);
        path.BezierCurveTo(640, -101, 567, -203, 469, -235);
        path.BezierCurveTo(475, -274, 481, -304, 484, -324);
        path.BezierCurveTo(494, -381, 500, -426, 500, -465);
        path.BezierCurveTo(500, -528, 486, -594, 432, -632);
        path.BezierCurveTo(396, -654, 355, -666, 308, -666);
        path.BezierCurveTo(173, -666, 122, -579, 122, -513);
        path.ClosePath();
        
        _ctx?.Fill(path);
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