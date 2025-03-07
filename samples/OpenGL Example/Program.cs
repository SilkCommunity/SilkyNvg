using NvgExample;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL;
using StbImageWriteSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;

namespace OpenGL_Example
{
    public class Program
    {

        private static GL gl;
        private static Nvg nvg;

        private static bool blowup = false;
        private static bool screenshot = false;
        private static bool premult = false;

        private static double prevTime = 0;
        private static double cpuTime = 0;

        private static PerformanceGraph frameGraph;
        private static PerformanceGraph cpuGraph;

        private static IWindow window;
        private static Demo demo;

        private static Stopwatch timer;

        private static float mx, my;

        private static unsafe void KeyDown(IKeyboard _, Key key, int _2)
        {
            if (key == Key.Escape)
                window.Close();
            else if (key == Key.Space)
                blowup = !blowup;
            else if (key == Key.S)
                screenshot = true;
            else if (key == Key.P)
                premult = !premult;
        }

        private static void MouseMove(IMouse _, System.Numerics.Vector2 mousePosition)
        {
            mx = mousePosition.X;
            my = mousePosition.Y;
        }

        private static void Load()
        {
            IInputContext input = window.CreateInput();
            foreach (IKeyboard keyboard in input.Keyboards)
            {
                keyboard.KeyDown += KeyDown;
            }
            foreach (IMouse mouse in input.Mice)
            {
                mouse.MouseMove += MouseMove;
            }

            gl = window.CreateOpenGL();

            OpenGLRenderer nvgRenderer = new(CreateFlags.Antialias | CreateFlags.StencilStrokes | CreateFlags.Debug, gl);
            nvg = Nvg.Create(nvgRenderer);

            demo = new Demo(nvg);

            timer = Stopwatch.StartNew();

            timer.Restart();

            prevTime = timer.Elapsed.TotalMilliseconds;
        }

        private static void Render(double _)
        {
            double t = timer.Elapsed.TotalSeconds;
            double dt = t - prevTime;
            prevTime = t;

            Vector2 winSize = window.Size.As<float>().ToSystem();
            Vector2 fbSize = window.FramebufferSize.As<float>().ToSystem();

            float pxRatio = fbSize.X / winSize.X;

            gl.Viewport(0, 0, (uint)fbSize.X, (uint)fbSize.Y);
            if (premult)
            {
                gl.ClearColor(0, 0, 0, 0);
            }
            else
            {
                gl.ClearColor(0.3f, 0.3f, 0.32f, 1.0f);
            }
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            nvg.BeginFrame((SizeF)winSize, pxRatio);

            demo.Render(mx, my, winSize.X, winSize.Y, (float)t, blowup);

            frameGraph.Render(5.0f, 5.0f, nvg);
            cpuGraph.Render(5.0f + 200.0f + 5.0f, 5.0f, nvg);

            nvg.EndFrame();

            cpuTime = timer.Elapsed.TotalSeconds - t;

            frameGraph.Update((float)dt);
            cpuGraph.Update((float)cpuTime);

            if (screenshot)
            {
                screenshot = false;
                SaveScreenShot((int)fbSize.X, (int)fbSize.Y, premult, "dump.png");
            }
        }

        private static void Close()
        {
            timer.Stop();

            demo.Dispose();

            nvg.Dispose();

            Console.WriteLine("Average Frame Time: " + frameGraph.GraphAverage * 1000.0f + " ms");
            Console.WriteLine("        CPU Time: " + cpuGraph.GraphAverage * 1000.0f + " ms");

            gl.Dispose();
        }

        static void Main()
        {
            frameGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");
            cpuGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Ms, "CPU Time");

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

        private static void UnpremultiplyAlpha(Span<byte> image, int w, int h, int stride)
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

        private static void SetAlpha(Span<byte> image, int w, int h, int stride, byte a)
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

        private static void FlipHorizontally(Span<byte> image, int w, int h, int stride)
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

        private static void SaveScreenShot(int w, int h, bool premult, string name)
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
