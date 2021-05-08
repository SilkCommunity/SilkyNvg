using Silk.NET.GLFW;
using Silk.NET.OpenGL.Legacy;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL.Legacy;
using System;

namespace SingleClassExample
{
    unsafe class Program
    {

        static Glfw glfw;
        static GL gl;

        static void ErrorCB(Silk.NET.GLFW.ErrorCode error, string desc)
        {
            Console.WriteLine("GLFW error: " + error + ": " + desc);
        }

        static void Key(WindowHandle* window, Keys key, int _, InputAction action, KeyModifiers __)
        {
            if (key == Keys.Escape && action == InputAction.Press)
                glfw.SetWindowShouldClose(window, true);
        }

        static void Main(string[] args)
        {
            WindowHandle* window;
            Nvg nvg;

            glfw = Glfw.GetApi();
            if (!glfw.Init())
            {
                Console.WriteLine("Failed to initialize GLFW!");
                Environment.Exit(-1);
            }

            glfw.SetErrorCallback(ErrorCB);

            glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
            glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
            glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

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
            gl.GetError();

            nvg = Nvg.Create(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, new LegacyOpenGLRenderer(gl));

            glfw.SwapInterval(0);

            glfw.SetTime(0);

            while (!glfw.WindowShouldClose(window))
            {
                glfw.GetCursorPos(window, out double mx, out double my);
                glfw.GetWindowSize(window, out int winWidth, out int winHeight);
                glfw.GetFramebufferSize(window, out int fbWidth, out int fbHeight);

                float pxRatio = (float)fbWidth / (float)winWidth;

                gl.Viewport(0, 0, (uint)fbWidth, (uint)fbHeight);

                gl.ClearColor(0, 0, 0, 0);
                gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit | (uint)ClearBufferMask.StencilBufferBit);

                glfw.SwapBuffers(window);
                glfw.PollEvents();
            }

            gl.Dispose();
            glfw.Terminate();
            glfw.Dispose();
        }
    }
}
