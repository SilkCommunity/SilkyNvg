using System.Diagnostics;
using NvgExample;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using SilkyNvg;
using SilkyNvg.Rendering.Vulkan;
using Veldrid;


public class Program
{

    private static Vector2D<int> windowExtent = new Vector2D<int>(1000, 600);
    private static IWindow window;

    private static bool blowup = false;
    private static bool premult = false;
    private static float mx, my;


    private static double prevTime = 0;
    private static double cpuTime = 0;

    private static Nvg nvg;


    private static GraphicsDevice device;
    private static CommandList list;
    private static NvgFrame nvgFrame;

    private static PerformanceGraph frameGraph;
    private static PerformanceGraph cpuGraph;

    private static Demo demo;

    private static Stopwatch timer;
    static VeldridRenderer veldridRenderer;


    private static void LoadInput()
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
    }


    private static void KeyDown(IKeyboard _, Key key, int _2)
    {
        if (key == Key.Escape)
            window.Close();
        else if (key == Key.Space)
            blowup = !blowup;
        else if (key == Key.P)
            premult = !premult;
    }

    private static void MouseMove(IMouse _, System.Numerics.Vector2 mousePosition)
    {
        mx = mousePosition.X;
        my = mousePosition.Y;
    }

    private static void FramebufferResize(Vector2D<int> size)
    {
        device.ResizeMainWindow((uint)size.X, (uint)size.Y);
        nvgFrame.Framebuffer = veldridRenderer.Device.SwapchainFramebuffer;


    }

    private static void LoadExample()
    {
        VeldridRendererParams @params = new()
        {
            AdvanceFrameIndexAutomatically = true,
            Device = device,
            InitialCommandBuffer = list

        };


        veldridRenderer = new(@params, RenderFlags.Antialias | RenderFlags.StencilStrokes);

        nvgFrame = new NvgFrame(new NvgFrameBufferParams()
        {
            Framebuffer = device.SwapchainFramebuffer,
        }, device);

        veldridRenderer.SetFrame(nvgFrame);

        nvg = Nvg.Create(veldridRenderer);

        demo = new Demo(nvg);

        timer = Stopwatch.StartNew();

        timer.Restart();

        prevTime = timer.Elapsed.TotalMilliseconds;
    }


    private static void Load()
    {
        LoadInput();
        LoadVeldrid();
        LoadExample();
    }

    private static void LoadVeldrid()
    {
        device = window.CreateGraphicsDevice(new GraphicsDeviceOptions(false, PixelFormat.D32_Float_S8_UInt, false, ResourceBindingModel.Default, true, true), GraphicsBackend.Vulkan);
        list = device.ResourceFactory.CreateCommandList();
    }

    public static void Main()
    {
        frameGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");
        cpuGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Ms, "CPU Time");

        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.FramesPerSecond = -1;
        windowOptions.Size = windowExtent;
        windowOptions.Title = "SilkyNvg";
        windowOptions.IsContextControlDisabled = true;
        windowOptions.VSync = false;
        windowOptions.PreferredBitDepth = new Vector4D<int>(32);
        windowOptions.PreferredDepthBufferBits = 32;
        windowOptions.PreferredStencilBufferBits = 8;
        windowOptions.API = GraphicsAPI.Default ;

        GraphicsAPI tempVer = GraphicsAPI.Default;
        tempVer.Version = new APIVersion(4, 0);
        windowOptions.API = tempVer ;


        window = Window.Create(windowOptions);

        window.Load += Load;
        window.FramebufferResize += FramebufferResize;
        window.Render += Render;
        window.Closing += Close;
        window.Run();
        

        window.Dispose();
    }

    private static void DisposeVeldrid()
    {
        list.Dispose();
        device.Dispose();
    }

    private static void Close()
    {
        timer.Stop();

        device.WaitForIdle();
        demo.Dispose();
        nvg.Dispose();
        nvgFrame.Dispose();
        window.Dispose();

        DisposeVeldrid();
    }

    private static void Render(double _)
    {


        CommandList cmd = list;


        BeginRender(cmd);
        ////////////////////////////////////////////////////////////
        double t = timer.Elapsed.TotalSeconds;
        double dt = t - prevTime;
        prevTime = t;

        Vector2D<float> wSize = window.Size.As<float>();
        Vector2D<float> fbSize = window.FramebufferSize.As<float>();

        float devicePxRatio = fbSize.X / wSize.X;

        nvg.BeginFrame(wSize, devicePxRatio);

        demo.Render(mx, my, wSize.X, wSize.Y, (float)t, blowup);

        frameGraph.Render(5.0f, 5.0f, nvg);
        cpuGraph.Render(5.0f + 200.0f + 5.0f, 5.0f, nvg);

        nvg.EndFrame();

        cpuTime = timer.Elapsed.TotalSeconds - t;

        frameGraph.Update((float)dt);
        cpuGraph.Update((float)cpuTime);

        ////////////////////////////////////////////////////////////
        EndRender(cmd);
        Present();

        //frameIndex++;
    }

    private static void BeginRender(CommandList cmd)
    {

        cmd.Begin();
        cmd.SetFramebuffer(device.SwapchainFramebuffer);

        if(premult)
        {
            cmd.ClearColorTarget(0, new RgbaFloat(0, 0, 0, 0));
        }
        else
        {
            cmd.ClearColorTarget(0, new RgbaFloat(0.3f, 0.3f, .32f, 1));
        }

        cmd.ClearDepthStencil(1, 0);
        cmd.SetViewport(0, new Viewport(0,0, windowExtent.X, windowExtent.Y, 0, 1));
        
    }

    private static void EndRender(CommandList cmd)
    {
        cmd.End();
        device.SubmitCommands(cmd);
    }

    private static void Present()
    {
        device.SwapBuffers();
    }
}