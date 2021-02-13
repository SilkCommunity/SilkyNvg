using Silk.NET.GLFW;
using Silk.NET.OpenGL;
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

            var nvg = Nvg.Create((uint)CreateFlags.Antialias | (uint)CreateFlags.StencilStrokes | (uint)CreateFlags.Debug, gl);

            if (nvg == null)
            {
                Console.Error.WriteLine("Could not initialize SilkyVG.");
                Environment.Exit(-1);
            }

            glfw.SwapInterval(0);

            while (!glfw.WindowShouldClose(window))
            {
                glfw.GetCursorPos(window, out double mx, out double my);
                glfw.GetWindowSize(window, out int winWidth, out int winHeight);

                float pxRatio = (float)winWidth / (float)winHeight;

                gl.Viewport(0, 0, (uint)winWidth, (uint)winHeight);
                gl.ClearColor(0, 0, 0, 1.0f);
                gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit | (uint)ClearBufferMask.StencilBufferBit);

                nvg.BeginFrame(winWidth, winHeight, pxRatio);

                nvg.BeginPath();
                nvg.Circle(50, 100, 100);
                nvg.FillColour(1.0f, 0.7f, 0.0f, 1.0f); // make this take colour object directly and also rgba!
                nvg.Stroke(0.0f, 0.5f, 1.0f, 1.0f, 10.0); // see above!

                nvg.EndFrame();

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            glfw.Terminate();
        }
    }
}
