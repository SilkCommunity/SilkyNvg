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

            PerformanceGraph fps = new(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");
            PerformanceGraph cpuGraph = new(PerformanceGraph.GraphRenderStyle.Ms, "CPU Time");

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

            Demo demo = new(nvg);

            glfw.SwapInterval(0);

            glfw.SetTime(0);
            prevt = glfw.GetTime();

            while (!glfw.WindowShouldClose(window))
            {
                double t = glfw.GetTime();
                double dt = t - prevt;
                prevt = t;

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
                    gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
                }
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

                nvg.BeginFrame(winWidth, winHeight, pxRatio); ;

                demo.Render((float)mx, (float)my, winWidth, winHeight, (float)t, blowup);

                fps.Render(5.0f, 5.0f, nvg);
                cpuGraph.Render(5.0f + 200.0f + 5.0f, 5.0f, nvg);

                nvg.EndFrame();

                cpuTime = glfw.GetTime() - t;

                fps.Update((float)dt);
                cpuGraph.Update((float)cpuTime);

                if (screenshot)
                {
                    screenshot = false;
                    demo.SaveScreenShot(fbWidth, fbHeight, premult, "dump.png");
                }

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            nvg.Dispose();

            Console.WriteLine("Average Frame Time: " + fps.GraphAverage * 1000.0f + " ms");
            Console.WriteLine("          CPU Time: " + cpuGraph.GraphAverage * 1000.0f + " ms");

            gl.Dispose();

            glfw.Terminate();
            glfw.Dispose();
        }
    }
}
