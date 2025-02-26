using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Extensions.Svg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering.OpenGL;

namespace OpenGL_Example
{
    public class Program
    {

        private static GL gl;
        private static Nvg nvg;

        private static IWindow window;

        private static string svgXml;

        private static void Load()
        {
            gl = window.CreateOpenGL();

            OpenGLRenderer nvgRenderer = new(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, gl);
            nvg = Nvg.Create(nvgRenderer);

            nvg.CreateSvgFromFile("./heart.svg");
        }

        private static void Render(double d)
        {
            Vector2D<float> winSize = window.Size.As<float>();
            Vector2D<float> fbSize = window.FramebufferSize.As<float>();

            float pxRatio = fbSize.X / winSize.X;

            gl.Viewport(0, 0, (uint)winSize.X, (uint)winSize.Y);
            gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            nvg.BeginFrame(winSize.As<float>(), pxRatio);

            float x = 100.0f, y = 100.0f;
            float w = 150.0f, h = 350.0f;
            float cornerRadius = 10.0f;

            nvg.BeginPath();
            nvg.Rect(x - 10.0f, y - 10.0f, w + 20.0f, h + 30.0f);
            nvg.PathWinding(Solidity.Hole);
            nvg.RoundedRect(x, y, w, h, cornerRadius);
            nvg.PathWinding(Solidity.Solid);
            nvg.FillColour(Colour.Yellow);
            nvg.Fill();

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
