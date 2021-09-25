using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Rendering.OpenGL;
using SilkyNvg.Text;

namespace OpenGL_Example
{
    public class Program
    {

        private static GL gl;
        private static Nvg nvg;

        private static IWindow window;

        private static float fontSize;

        private static void KeyDown(IKeyboard _, Key key, int _2)
        {
            if (key == Key.Escape)
                window.Close();
        }

        private static void Load()
        {
            IInputContext input = window.CreateInput();
            foreach (IKeyboard keyboard in input.Keyboards)
            {
                keyboard.KeyDown += KeyDown;
            }

            gl = window.CreateOpenGL();

            OpenGLRenderer nvgRenderer = new(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, gl);
            nvg = Nvg.Create(nvgRenderer);

            fontSize = 20.0f;
            _ = nvg.CreateFont("Roboto", "Roboto-Regular.ttf");
        }

        private static void DrawFontIncreasing(string text, float x, float y, float h, float delta)
        {
            nvg.FontSize(fontSize);
            fontSize += 0.5f;

            nvg.FontFace("Roboto");
            nvg.FillColour(nvg.Rgba(255, 255, 255, 128));

            nvg.TextAlign(Align.Left | Align.Middle);
            _ = nvg.Text(x, y + (h * 0.5f), text);
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

            DrawFontIncreasing("The quick brown fox...", 250.0f, 250.0f, 10.0f, delta);

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
