using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using NvgExample;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering.Veldrid;
using Veldrid;
using Veldrid.OpenGL;
using VeldridCompat = SilkyNvg.Rendering.Veldrid.VeldridCompat;

/// <summary>
/// Standalone Veldrid example for the SilkyNvg VeldridRenderer backend.
/// Uses Silk.NET windowing (same as the OpenGL example) for proper DPI awareness.
/// Creates a Veldrid GraphicsDevice from the native window handle.
/// </summary>
public static class Program
{
    private const int WINDOW_WIDTH = 1000;
    private const int WINDOW_HEIGHT = 600;

    private static GraphicsBackend selectedGraphicsBackend = GraphicsBackend.Direct3D11;
    private static IWindow appWindow = null!;
    private static GraphicsDevice graphicsDevice = null!;
    private static CommandList renderCommandList = null!;
    private static VeldridRenderer nvgRenderer = null!;
    private static Nvg nvgContext = null!;
    private static Demo demo = null!;

    private static PerformanceGraph frameTimeGraph = null!;
    private static PerformanceGraph cpuTimeGraph = null!;
    private static Stopwatch frameTimer = null!;
    private static double previousTimeSeconds;
    private static float mouseX, mouseY;
    private static bool blowup;

    private static void Load()
    {
        // Create Veldrid GraphicsDevice
        GraphicsDeviceOptions graphicsDeviceOptions = new GraphicsDeviceOptions
        {
            Debug = true,  // Enable GL debug output to surface shader link errors
            PreferDepthRangeZeroToOne = true,
            PreferStandardClipSpaceYDirection = true,
            SyncToVerticalBlank = false,  // Disable vsync for performance comparison
            // 8-bit stencil required for non-convex path fill (stencil-then-cover)
            SwapchainDepthFormat = VeldridCompat.DepthStencilD24S8
        };

        Vector2D<int> framebufferSize = appWindow.FramebufferSize;

        if (selectedGraphicsBackend == GraphicsBackend.OpenGL)
        {
            // OpenGL: use Silk.NET's GL context via OpenGLPlatformInfo
            var glContext = appWindow.GLContext;
            if (glContext == null)
            {
                throw new InvalidOperationException(
                    "GLContext is null. Ensure windowOptions.API is set to create an OpenGL context.");
            }

            // Wire Veldrid's OpenGL backend to Silk.NET's IGLContext
            // This allows Veldrid's execution thread to properly manage the GL context
            OpenGLPlatformInfo openGlPlatformInfo = new OpenGLPlatformInfo(
                openGLContextHandle: glContext.Handle,
                getProcAddress: name => glContext.GetProcAddress(name),
                makeCurrent: ctx =>
                {
                    Console.WriteLine($"[GL CONTEXT] MakeCurrent called on thread {Environment.CurrentManagedThreadId}");
                    glContext.MakeCurrent();
                },
                getCurrentContext: () => glContext.IsCurrent ? glContext.Handle : IntPtr.Zero,
                clearCurrentContext: () =>
                {
                    Console.WriteLine($"[GL CONTEXT] ClearCurrentContext called on thread {Environment.CurrentManagedThreadId}");
                    glContext.Clear();
                },
                deleteContext: _ => { }, // No-op: Silk.NET owns the context, will dispose it
                swapBuffers: () => glContext.SwapBuffers(),
                setSyncToVerticalBlank: vsync => glContext.SwapInterval(vsync ? 1 : 0));

            graphicsDevice = GraphicsDevice.CreateOpenGL(
                graphicsDeviceOptions,
                openGlPlatformInfo,
                (uint)framebufferSize.X,
                (uint)framebufferSize.Y);

            // Neutralize Silk.NET's context management - Veldrid's execution thread now owns it
            // This prevents conflicts where both Silk.NET and Veldrid try to MakeCurrent
            glContext.Clear();
        }
        else
        {
            // D3D11/Vulkan: use native window handle via SwapchainDescription
            SwapchainSource swapchainSource = GetSwapchainSource(appWindow);
            SwapchainDescription swapchainDescription = new SwapchainDescription(
                swapchainSource,
                (uint)framebufferSize.X,
                (uint)framebufferSize.Y,
                graphicsDeviceOptions.SwapchainDepthFormat,
                graphicsDeviceOptions.SyncToVerticalBlank);

            graphicsDevice = selectedGraphicsBackend switch
            {
                GraphicsBackend.Direct3D11 => GraphicsDevice.CreateD3D11(graphicsDeviceOptions, swapchainDescription),
                GraphicsBackend.Vulkan => GraphicsDevice.CreateVulkan(graphicsDeviceOptions, swapchainDescription),
                _ => throw new ArgumentException(
                    $"Unsupported graphics backend: {selectedGraphicsBackend}. " +
                    "Supported: --d3d11, --vulkan, --opengl")
            };
        }

        Console.WriteLine($"Graphics backend: {graphicsDevice.BackendType}");
        appWindow.Title = $"SilkyNvg {VeldridCompat.PackageName} Backend Example ({graphicsDevice.BackendType})";

        // Diagnostic: compare window logical size vs framebuffer physical size
        uint framebufferPixelWidth = graphicsDevice.SwapchainFramebuffer.Width;
        uint framebufferPixelHeight = graphicsDevice.SwapchainFramebuffer.Height;
        Vector2D<int> windowLogicalSize = appWindow.Size;
        Console.WriteLine($"[VELDRID DIAG] Window logical size: {windowLogicalSize.X}x{windowLogicalSize.Y}");
        Console.WriteLine($"[VELDRID DIAG] Framebuffer pixel size: {framebufferPixelWidth}x{framebufferPixelHeight}");
        Console.WriteLine($"[VELDRID DIAG] Pixel ratio (fb/win): {(float)framebufferPixelWidth / windowLogicalSize.X:F3}x{(float)framebufferPixelHeight / windowLogicalSize.Y:F3}");

        // Create Veldrid command list
        renderCommandList = graphicsDevice.ResourceFactory.CreateCommandList();

        // Create our NVG renderer and context
        nvgRenderer = new VeldridRenderer(graphicsDevice);
        nvgRenderer.Create();
        nvgRenderer.SetActiveCommandList(renderCommandList);

        nvgContext = Nvg.Create(nvgRenderer);

        // Load the shared NvgExample demo (fonts, images, UI widgets)
        demo = new Demo(nvgContext);

        frameTimeGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");
        cpuTimeGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Ms, "CPU Time");

        frameTimer = Stopwatch.StartNew();
        previousTimeSeconds = 0;

        // Wire up input (mouse + keyboard) — same as OpenGL example
        IInputContext inputContext = appWindow.CreateInput();
        foreach (IKeyboard keyboard in inputContext.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
        }
        foreach (IMouse mouse in inputContext.Mice)
        {
            mouse.MouseMove += OnMouseMove;
        }

        Console.WriteLine("Entering render loop. Close window to exit.");
    }

    private static void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        if (key == Key.Escape)
        {
            appWindow.Close();
        }
        else if (key == Key.Space)
        {
            blowup = !blowup;
        }
    }

    private static void OnMouseMove(IMouse mouse, Vector2 mousePosition)
    {
        mouseX = mousePosition.X;
        mouseY = mousePosition.Y;
    }

    private static void Render(double deltaTime)
    {
        // Timing
        double currentTimeSeconds = frameTimer!.Elapsed.TotalSeconds;
        double frameDeltaSeconds = currentTimeSeconds - previousTimeSeconds;
        previousTimeSeconds = currentTimeSeconds;
        
        // Use provided deltaTime if non-zero (from window.Run), otherwise use calculated
        if (deltaTime > 0) {
            frameDeltaSeconds = deltaTime;
        }

        // Begin GPU commands
        renderCommandList.Begin();
        renderCommandList.SetFramebuffer(graphicsDevice.SwapchainFramebuffer);
        renderCommandList.ClearColorTarget(0, new RgbaFloat(0.3f, 0.3f, 0.32f, 1.0f));
        renderCommandList.ClearDepthStencil(1f, 0);

        // NVG frame — use window size (logical) for NVG coordinates
        Vector2 windowSize = appWindow.Size.As<float>().ToSystem();
        nvgContext.BeginFrame(new SizeF(windowSize.X, windowSize.Y), 1.0f);

        // Draw the full NanoVG demo (eyes, widgets, color wheel, graphs, etc.)
        demo.Render(mouseX, mouseY, windowSize.X, windowSize.Y, (float)currentTimeSeconds, blowup);

        // Performance graphs
        frameTimeGraph.Render(5.0f, 5.0f, nvgContext);
        cpuTimeGraph.Render(5.0f + 200.0f + 5.0f, 5.0f, nvgContext);

        nvgContext.EndFrame();

        double cpuTimeSeconds = frameTimer.Elapsed.TotalSeconds - currentTimeSeconds;

        frameTimeGraph.Update((float)frameDeltaSeconds);
        cpuTimeGraph.Update((float)cpuTimeSeconds);

        // Submit
        renderCommandList.End();
        graphicsDevice.SubmitCommands(renderCommandList);
        graphicsDevice.SwapBuffers();
    }

    private static void Close()
    {
        frameTimer!.Stop();

        Console.WriteLine($"Average Frame Time: {frameTimeGraph.GraphAverage * 1000.0f} ms");
        Console.WriteLine($"        CPU Time: {cpuTimeGraph.GraphAverage * 1000.0f} ms");

        Console.WriteLine("Shutting down...");
        graphicsDevice.WaitForIdle();
        demo.Dispose();
        nvgContext.Dispose();
        nvgRenderer.Dispose();
        renderCommandList.Dispose();
        graphicsDevice.Dispose();
    }

    private static void Resize(Vector2D<int> newSize)
    {
        graphicsDevice.MainSwapchain.Resize((uint)newSize.X, (uint)newSize.Y);
    }

    public static void Main(string[] args)
    {
        // Parse command-line backend selection: --d3d11, --vulkan, --opengl
        selectedGraphicsBackend = GraphicsBackend.Direct3D11; // default
        foreach (string arg in args)
        {
            switch (arg.ToLowerInvariant())
            {
                case "--d3d11":
                case "--d3d":
                    selectedGraphicsBackend = GraphicsBackend.Direct3D11;
                    break;
                case "--vulkan":
                case "--vk":
                    selectedGraphicsBackend = GraphicsBackend.Vulkan;
                    break;
                case "--opengl":
                case "--gl":
                    selectedGraphicsBackend = GraphicsBackend.OpenGL;
                    break;
            }
        }
        Console.WriteLine($"Selected backend: {selectedGraphicsBackend}");

        WindowOptions windowOptions = WindowOptions.Default;
        windowOptions.Size = new Vector2D<int>(WINDOW_WIDTH, WINDOW_HEIGHT);
        windowOptions.Title = $"SilkyNvg {VeldridCompat.PackageName} Backend Example ({selectedGraphicsBackend})";
        windowOptions.VSync = false;  // Disable vsync for performance comparison
        windowOptions.PreferredDepthBufferBits = 24;
        windowOptions.PreferredStencilBufferBits = 8;

        // OpenGL requires Silk.NET to create the GL context, others use Veldrid's swapchain
        if (selectedGraphicsBackend == GraphicsBackend.OpenGL)
        {
            windowOptions.API = GraphicsAPI.Default; // Let Silk.NET create OpenGL 3.3+ context
            windowOptions.ShouldSwapAutomatically = false; // Veldrid will handle swapbuffers
        }
        else
        {
            windowOptions.API = GraphicsAPI.None; // D3D11/Vulkan create their own swapchain
        }

        appWindow = Window.Create(windowOptions);

        // For OpenGL: use manual event pump to prevent Silk.NET from fighting with Veldrid for context ownership
        // For D3D11/Vulkan: use normal Run() since they don't share GL context
        if (selectedGraphicsBackend == GraphicsBackend.OpenGL)
        {
            appWindow.Initialize();

            // Make GL context current on main thread for Veldrid initialization
            // Veldrid needs this to load GL function pointers via getProcAddress
            appWindow.GLContext!.MakeCurrent();

            Load(); // Creates Veldrid device, which will clear context and transfer to execution thread
            
            // Ensure all deferred resource creation (shaders, pipelines, buffers) is flushed
            // before the first frame tries to use them. On the OpenGL backend, Create* calls
            // are queued to the execution thread and may not be ready yet.
            graphicsDevice.WaitForIdle();
            
            // Diagnostic: verify stencil format — NVG's stencil-then-cover fill requires D24_UNorm_S8_UInt
            var depthAttachment = graphicsDevice.SwapchainFramebuffer.OutputDescription.DepthAttachment;
            Console.WriteLine($"[DIAG] Backend: {graphicsDevice.BackendType}");
            Console.WriteLine($"[DIAG] Depth/stencil format: {depthAttachment?.Format}");
            if (depthAttachment == null) {
                Console.WriteLine("[DIAG] WARNING: No depth/stencil attachment! NVG stencil fill will not work.");
            }
            
            // Manual event loop - Veldrid owns the GL context exclusively
            Console.WriteLine("[MANUAL LOOP] Starting render loop");
            while (!appWindow.IsClosing)
            {
                appWindow.DoEvents();
                
                // Calculate delta time for this frame
                double currentTime = frameTimer.Elapsed.TotalSeconds;
                double deltaTime = currentTime - previousTimeSeconds;
                
                Render(deltaTime);
            }
            Console.WriteLine("[MANUAL LOOP] Exited render loop");

            Close();
            appWindow.DoEvents(); // Final event processing
        }
        else
        {
            appWindow.Load += Load;
            appWindow.Render += Render;
            appWindow.Closing += Close;
            appWindow.Resize += Resize;
            appWindow.Run();
        }

        appWindow.Dispose();
    }

    /// <summary>
    /// Creates a Veldrid SwapchainSource from a Silk.NET window's native handle.
    /// Supports Win32 (Windows) — extend for other platforms as needed.
    /// </summary>
    private static SwapchainSource GetSwapchainSource(IWindow silkNetWindow)
    {
        if (silkNetWindow.Native == null)
        {
            throw new InvalidOperationException(
                "Silk.NET window has no native handle. " +
                "Ensure the window is created with API = GraphicsAPI.None and is fully initialized.");
        }

        if (silkNetWindow.Native.Win32.HasValue)
        {
            (nint hwnd, nint hdc, nint hinstance) = silkNetWindow.Native.Win32.Value;
            return SwapchainSource.CreateWin32(hwnd, hinstance);
        }

        // Add X11/Wayland/macOS support here if needed
        throw new PlatformNotSupportedException(
            $"No supported native window handle found. " +
            $"Win32={silkNetWindow.Native.Win32.HasValue}. " +
            $"Only Windows (Win32) is currently supported for the Veldrid example.");
    }
}