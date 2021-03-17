using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using SilkyNvg;
using SilkyNvg.Base;
using SilkyNvg.Colouring;
using System;
using SilkyNvg.Core;

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

            gl.GetInteger(GLEnum.ContextFlags, out int flags);
            if ((flags & (int)GLEnum.ContextFlagDebugBit) != 0)
            {
                gl.Enable(EnableCap.DebugOutput);
                gl.Enable(EnableCap.DebugOutputSynchronous);
                gl.DebugMessageCallback((source, type, id, severity, length, message, userparam) =>
                {
                    if (id == 131169 || id == 131185 || id == 131218 || id == 131204  || id == 1282) return;

                    Console.WriteLine("------------------------------------------");
                    Console.WriteLine("Debug message (" + id + "): " + Silk.NET.Core.Native.SilkMarshal.PtrToString(message) + " - " + source + " - " + type + " - " + severity);
                }, null);
                gl.DebugMessageControl(DebugSource.DontCare, DebugType.DontCare, DebugSeverity.DontCare, 0, null, true);
            }

            // Create a new NVG context.
            var nvg = Nvg.Create((uint)CreateFlag.Debug, gl);

            glfw.SwapInterval(0);

            while (!glfw.WindowShouldClose(window))
            {
                glfw.GetCursorPos(window, out double mx, out double my);
                glfw.GetWindowSize(window, out int winWidth, out int winHeight);
                glfw.GetFramebufferSize(window, out int fbWidth, out int fbHeight);

                float pxRatio = (float)fbWidth / (float)winWidth;

                gl.Viewport(0, 0, (uint)winWidth, (uint)winHeight);
                gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
                gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit | (uint)ClearBufferMask.StencilBufferBit);

                nvg.BeginFrame(winWidth, winHeight, fbWidth / winWidth);

                nvg.BeginPath();

                nvg.Shapes.Rect(100, 100, 300, 500);
                nvg.StrokeColour(Colour.Blue);
                nvg.Stroke();
                nvg.BeginPath();
                nvg.Shapes.Rect(600, 100, 300, 500);
                nvg.Stroke();

                nvg.EndFrame();

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            nvg.Delete();

            glfw.Terminate();
        }
    }
}
