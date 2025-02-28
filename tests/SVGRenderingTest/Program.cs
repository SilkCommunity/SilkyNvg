using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Extensions.Svg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering.OpenGL;
using SilkyNvg.Transforms;

namespace OpenGL_Example
{
    public class Program
    {

        private static GL gl;
        private static Nvg nvg;

        private static IWindow window;

        private static SvgImage svg;

        private static void Load()
        {
            gl = window.CreateOpenGL();

            OpenGLRenderer nvgRenderer = new(CreateFlags.StencilStrokes | CreateFlags.Debug, gl);
            nvg = Nvg.Create(nvgRenderer);

            svg = nvg.CreateSvgFromFile("./heart.svg") ?? throw new InvalidOperationException("Failed to parser Svg");
        }

        static double dl = 0;

        private static void Render(double d)
        {
            Vector2D<float> winSize = window.Size.As<float>();
            Vector2D<float> fbSize = window.FramebufferSize.As<float>();

            float pxRatio = fbSize.X / winSize.X;

            gl.Viewport(0, 0, (uint)winSize.X, (uint)winSize.Y);
            gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            nvg.BeginFrame(winSize.As<float>(), pxRatio);

            nvg.BeginPath();

            var q0 = new Vector2D<float>(200.0f, 300.0f);
            var q1 = new Vector2D<float>(400.0f, 50.0f);
            var q2 = new Vector2D<float>(600.0f, 300.0f);

            var c0 = q0;
            var c3 = q2;
            var c1 = q0 + 2.0f / 3.0f * (q1 - q0);
            var c2 = q2 + 2.0f / 3.0f * (q1 - q2);

            nvg.MoveTo(c0);
            nvg.BezierTo(c1, c2, c3);

            nvg.StrokeWidth(2.0f);
            nvg.StrokeColour(Colour.Yellow);
            nvg.Stroke();

            nvg.DrawSvgImage(svg);

            nvg.EndFrame();
        }

        private static void Close()
        {
            nvg.Dispose();
            gl.Dispose();
        }

        static void Main()
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.FramesPerSecond = -1;
            windowOptions.ShouldSwapAutomatically = true;
            windowOptions.Size = new Vector2D<int>(1000, 600);
            windowOptions.Title = "SilkyNvg";
            windowOptions.VSync = false;
            windowOptions.PreferredDepthBufferBits = 24;
            windowOptions.PreferredStencilBufferBits = 8;

            window = Window.Create(windowOptions);
            window.Load += Load;
            window.Render += Render;
            window.Closing += Close;
            window.Run();

            window.Dispose();
        }

    }
}
