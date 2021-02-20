using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SilkyNvg;
using System;

namespace SingleClassExample
{
    class Program
    {

        private static void Error(Silk.NET.GLFW.ErrorCode code, string message)
        {
            Console.Error.WriteLine("GLFW Error: " + code + ": " + message + ".");
        }

        static unsafe void Main(string[] args)
        {
            var glfw = Glfw.GetApi();
            WindowHandle* window;

            if (!glfw.Init())
            {
                Console.Error.WriteLine("Failed to initialize glfw!");
                Environment.Exit(-1);
            }

            glfw.SetErrorCallback(Error);

            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
            glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

            glfw.WindowHint(WindowHintBool.OpenGLDebugContext, true);

            glfw.WindowHint(WindowHintInt.Samples, 4);

            window = glfw.CreateWindow(1280, 720, "Single class example", null, null);

            if (window == null)
            {
                glfw.Terminate();
                Environment.Exit(-1);
            }

            glfw.MakeContextCurrent(window);

            var context = new GlfwContext(glfw, window);
            var gl = GL.GetApi(context);

            gl.GetError();

            // Create a new NVG context.
            var nvg = Nvg.Create((uint)CreateFlag.EdgeAntialias | (uint)CreateFlag.StencilStrokes | (uint)CreateFlag.Debug, gl);

            glfw.SwapInterval(0);

            while (!glfw.WindowShouldClose(window))
            {
                glfw.GetCursorPos(window, out double mx, out double my);
                glfw.GetWindowSize(window, out int winWidth, out int winHeight);
                glfw.GetFramebufferSize(window, out int fbWidth, out int fbHeight);

                float pxRatio = (float)fbWidth / (float)winWidth;

                gl.Viewport(0, 0, (uint)winWidth, (uint)winHeight);
                gl.ClearColor(0, 0, 0, 1.0f);
                gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit | (uint)ClearBufferMask.StencilBufferBit);

                nvg.BeginFrame(winWidth, winHeight, pxRatio);

                var pos = new Vector2D<float>(200.0f, 200.0f);
                nvg.BeginPath();
                nvg.Circle(pos, 50);
                nvg.FillColour(nvg.FromRGBAf(0.0f, 0.3f, 0.8f, 1.0f));
                /*nvg.Fill();

                nvg.EndFrame();*/

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            // nvg.Delete();

            glfw.Terminate();
        }
    }
}
