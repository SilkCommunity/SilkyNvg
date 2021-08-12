using NvgExample;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL;
using StbImageWriteSharp;
using System;
using System.IO;

namespace OpenGL_Example
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

                nvg.BeginFrame(winWidth, winHeight, pxRatio);

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
                    SaveScreenShot((int)fbWidth, (int)fbHeight, premult, "dump.png");
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

        static void UnpremultiplyAlpha(Span<byte> image, int w, int h, int stride)
        {
            for (int y = 0; y < h; y++)
            {
                Span<byte> row = image[(y * stride)..];
                for (int x = 0; x < w; x += 4)
                {
                    byte r = row[x + 0];
                    byte g = row[x + 1];
                    byte b = row[x + 2];
                    byte a = row[x + 3];
                    if (a != 0)
                    {
                        row[x + 0] = (byte)Math.Min(r * 255 / a, 255);
                        row[x + 1] = (byte)Math.Min(g * 255 / a, 255);
                        row[x + 2] = (byte)Math.Min(b * 255 / a, 255);
                    }
                }
            }

            for (int y = 0; y < h; y++)
            {
                int offset = y * stride;
                Span<byte> row = image[offset..];
                for (int x = 0; x < w; x++)
                {
                    byte r = 0, g = 0, b = 0;
                    byte a = row[3];
                    byte n = 0;
                    if (a == 0)
                    {
                        if (x - 1 > 0 && image[offset - 1] != 0)
                        {
                            r += image[offset - 4];
                            g += image[offset - 3];
                            b += image[offset - 2];
                            n++;
                        }
                        if (x + 1 < w && row[7] != 0)
                        {
                            r += row[4];
                            g += row[5];
                            b += row[6];
                            n++;
                        }
                        if (y - 1 > 0 && image[offset - stride + 3] != 0)
                        {
                            r += image[offset - stride];
                            g += image[offset - stride + 1];
                            b += image[offset - stride + 2];
                            n++;
                        }
                        if (y + 1 < h && row[stride + 3] != 0)
                        {
                            r += row[stride];
                            g += row[stride + 1];
                            b += row[stride + 2];
                            n++;
                        }
                        if (n > 0)
                        {
                            row[0] = (byte)(r / n);
                            row[1] = (byte)(g / n);
                            row[2] = (byte)(b / n);
                        }
                    }
                    row = image[(offset + x * 4)..];
                }
            }
        }

        static void SetAlpha(Span<byte> image, int w, int h, int stride, byte a)
        {
            for (int y = 0; y < h; y++)
            {
                Span<byte> row = image[(y * stride)..];
                for (int x = 0; x < w; x++)
                {
                    row[x * 4 + 3] = a;
                }
            }
        }

        static void FlipHorizontally(Span<byte> image, int w, int h, int stride)
        {
            int i = 0;
            int j = h - 1;
            while (i < j)
            {
                Span<byte> ri = image[(i * stride)..];
                Span<byte> rj = image[(j * stride)..];
                for (int k = 0; k < w * 4; k++)
                {
                    byte t = ri[k];
                    ri[k] = rj[k];
                    rj[k] = t;
                }
                i++;
                j--;
            }
        }

        static void SaveScreenShot(int w, int h, bool premult, string name)
        {
            Span<byte> image = new byte[w * h * 4];
            gl.ReadPixels(0, 0, (uint)w, (uint)h, GLEnum.Rgba, GLEnum.UnsignedByte, image);
            if (premult)
            {
                UnpremultiplyAlpha(image, w, h, w * 4);
            }
            else
            {
                SetAlpha(image, w, h, w * 4, 255);
            }
            FlipHorizontally(image, w, h, w * 4);
            ImageWriter imageWriter = new();
            imageWriter.WritePng(image.ToArray(), w, h, ColorComponents.RedGreenBlueAlpha, File.OpenWrite(name));
        }
    }
}
