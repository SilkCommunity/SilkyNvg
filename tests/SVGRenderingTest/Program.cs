using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
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

        private static void Load()
        {
            gl = window.CreateOpenGL();

            OpenGLRenderer nvgRenderer = new(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, gl);
            nvg = Nvg.Create(nvgRenderer);
        }

        private static void Render(double d)
        {
            float delta = (float)d;

            Vector2D<float> winSize = window.Size.As<float>();
            Vector2D<float> fbSize = window.FramebufferSize.As<float>();

            float pxRatio = fbSize.X / winSize.X;

            gl.Viewport(0, 0, (uint)winSize.X, (uint)winSize.Y);
            gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            nvg.BeginFrame(winSize.As<float>(), pxRatio);

            nvg.BeginPath();
            nvg.Rect(10.0f, 10.0f, 50.0f, 50.0f);
            nvg.FillColour(Colour.BlueViolet);
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
