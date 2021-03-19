using Silk.NET.OpenGL;
using Silk.NET.GLFW;
using SilkyNvg;
using SilkyNvg.Base;
using System;

namespace NvgExample
{
    unsafe class Example
    {

        private WindowHandle* _window;
        private Nvg _nvg;
        private Demo _demo;
        private GPUTimer _gpuTimer;

        private Glfw _glfw;
        private GL _gl;

        private bool _premult, _blowup, _screenshot;

        private Example((int, int) size)
        {
            Run(size);
        }

        private void Errorcb(Silk.NET.GLFW.ErrorCode error, string message)
        {
            Console.Error.WriteLine("GLFW Error: ", error, " " + message);
        }

        private void Key(WindowHandle* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
        {
            if (key == Keys.Escape && action == InputAction.Press)
                _glfw.SetWindowShouldClose(window, true);
            if (key == Keys.Space && action == InputAction.Press)
                _blowup = !_blowup;
            if (key == Keys.S && action == InputAction.Press)
                _screenshot = true;
            if (key == Keys.P && action == InputAction.Press)
                _premult = !_premult;
        }

        private void Run((int, int) size)
        {
            _glfw = Glfw.GetApi();
            double prevt = 0, cpuTime = 0;

            if (!_glfw.Init())
            {
                Console.Error.WriteLine("Failed to initialize GLFW!");
                Environment.Exit(-1);
            }

            _glfw.SetErrorCallback(Errorcb);

            _glfw.WindowHint(WindowHintInt.ContextVersionMajor, 3);
            _glfw.WindowHint(WindowHintInt.ContextVersionMinor, 2);
            _glfw.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
            _glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);

            _glfw.WindowHint(WindowHintBool.OpenGLDebugContext, true);

            _glfw.WindowHint(WindowHintInt.Samples, 4);

            _window = _glfw.CreateWindow(1280, 720, "Single class example", null, null);

            if (_window == null)
            {
                _glfw.Terminate();
                Environment.Exit(-1);
            }

            _glfw.SetKeyCallback(_window, Key);

            _glfw.MakeContextCurrent(_window);

            var context = new GlfwContext(_glfw, _window);
            _gl = GL.GetApi(context);
            _gl.GetError();

            _nvg = Nvg.Create((uint)CreateFlag.Antialias | (uint)CreateFlag.StencilStrokes | (uint)CreateFlag.Debug, _gl);

            _demo = new Demo(_nvg, _gl);

            _glfw.SwapInterval(0);

            _gpuTimer = new GPUTimer(_gl);

            _glfw.SetTime(0);
            prevt = _glfw.GetTime();

            while (!_glfw.WindowShouldClose(_window))
            {
                double t = _glfw.GetTime();
                double dt = t - prevt;
                prevt = t;

                _gpuTimer.Start();

                _glfw.GetCursorPos(_window, out double mx, out double my);
                _glfw.GetWindowSize(_window, out int winWidth, out int winHeight);
                _glfw.GetFramebufferSize(_window, out int fbWidth, out int fbHeight);

                float pxRatio = (float)fbWidth / (float)winWidth;

                _gl.Viewport(0, 0, (uint)fbWidth, (uint)fbHeight);

                if (_premult)
                    _gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
                else
                    _gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
                _gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit | (uint)ClearBufferMask.StencilBufferBit);

                _nvg.BeginFrame((float)winWidth, (float)winHeight, pxRatio);

                _demo.Render((float)mx, (float)my, winWidth, winHeight, (float)t, _blowup);

                _nvg.EndFrame();

                cpuTime = _glfw.GetTime() - t;

                _glfw.SwapBuffers(_window);
                _glfw.PollEvents();
            }

            _nvg.Delete();

            Console.WriteLine("Averege Frame Time: ");
            Console.WriteLine("        CPU Time: ");
            Console.WriteLine("        GPU Time: ");

            _glfw.Terminate();

        }

        static void Main()
        {
            new Example((1000, 600));
        }

    }
}
