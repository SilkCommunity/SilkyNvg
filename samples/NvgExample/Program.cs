using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Rendering.OpenGL;
using System;

namespace NvgExample
{
    unsafe class Program
    {

        static void Errorcb(Silk.NET.GLFW.ErrorCode error, string desc)
        {
            Console.Error.WriteLine("GLFW error: " + error + Environment.NewLine + desc);
        }

        static Glfw glfw;
        static GL gl;
        static Nvg nvg;

        static bool blowup = false;
        static bool screenshot = false;
        static bool premult = false;

        static void Key(WindowHandle* window, Keys key, int _, InputAction action, KeyModifiers __)
        {
            if (key == Keys.Escape && action == InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
            if (key == Keys.Space && action == InputAction.Press)
                blowup = !blowup;
            if (key == Keys.S && action == InputAction.Press)
                screenshot = true;
            if (key == Keys.P && action == InputAction.Press)
                premult = !premult;

        }

        static void Main()
        {
            glfw = Glfw.GetApi();
            WindowHandle* window;

            double prevt = 0, cpuTime = 0;

            if (!glfw.Init())
            {
                Console.Error.WriteLine("Failed to init GLFW.");
                Environment.Exit(-1);
            }

            // TODO: Init graph

            glfw.SetErrorCallback(Errorcb);

            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
            glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

            glfw.WindowHint(WindowHintBool.OpenGLDebugContext, true);

            glfw.WindowHint(WindowHintInt.Samples, 4);

            window = glfw.CreateWindow(1000, 600, "SilkyNvg", null, null);
            if (window == null)
            {
                glfw.Terminate();
                Environment.Exit(-1);
            }

            glfw.SetKeyCallback(window, Key);

            glfw.MakeContextCurrent(window);

            gl = GL.GetApi(new GlfwContext(glfw, window));
            _ = gl.GetError();

            nvg = Nvg.Create(new OpenGLRenderer(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, gl));

            // TODO: Load demo data

            glfw.SwapInterval(0);

            // TODO: Init GPU timer

            glfw.SetTime(0);
            prevt = glfw.GetTime();

            while (!glfw.WindowShouldClose(window))
            {
                double t = glfw.GetTime();
                double dt = t - prevt;
                prevt = t;

                // TODO: Timer

                glfw.GetCursorPos(window, out double mx, out double my);
                glfw.GetWindowSize(window, out int winWidth, out int winHeight);
                glfw.GetFramebufferSize(window, out int fbWidth, out int fbHeight);

                float pxRatio = (float)fbWidth / (float)winWidth;

                gl.Viewport(0, 0, (uint)winWidth, (uint)winHeight);
                if (premult)
                {
                    gl.ClearColor(0, 0, 0, 0);
                }
                else
                {
                    gl.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
                }
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                nvg.BeginFrame(winWidth, winHeight, pxRatio); ;

                // Render all the stuff

                nvg.BeginPath();
                nvg.Rect(100, 100, 50, 50);
                nvg.StrokeColour(Colour.White);
                nvg.Stroke();

                nvg.BeginPath();
                nvg.MoveTo(200, 200);
                nvg.LineTo(200, 250);
                nvg.LineTo(250, 200);
                nvg.ClosePath();
                nvg.StrokeColour(new Colour(0, 0, 255));
                nvg.Stroke();

                nvg.EndFrame();

                cpuTime = glfw.GetTime() - t;

                // TODO: Graph N timer

                // TODO: Screenshot

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }
        }
    }
}
