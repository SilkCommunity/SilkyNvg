// Android Veldrid example (C)opyuright 2026 by David Jeske <davidj@gmail.com>
// with assistance from Claude Sonnet 4.5 and Claude Opus 4.6
// Released under the MIT License.

#nullable enable
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Android.Util;
using NvgExample;
using SilkyNvg;
using SilkyNvg.Blending;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Text;
using SilkyNvg.Transforms;
using SilkyNvg.Rendering.Veldrid;
using Veldrid;
using VeldridCompat = SilkyNvg.Rendering.Veldrid.VeldridCompat;

namespace Veldrid_Android_Example;

/// <summary>
/// Creates a Veldrid GraphicsDevice from an Android Surface (via Vulkan),
/// initializes the SilkyNvg VeldridRenderer, and runs the NanoVG demo render loop
/// on a dedicated background thread.
/// </summary>
public class VeldridAndroidRenderer
{
    private const string LOG_TAG = "NvgVeldrid";

    // Backend selection and device management
    private GraphicsBackend _currentBackend = GraphicsBackend.Vulkan;
    private GraphicsDevice? _graphicsDevice;
    private CommandList? renderCommandList;
    private VeldridRenderer? nvgVeldridRenderer;
    private Nvg? nvgContext;
    private Demo? nvgDemo;
    private int _backendLabelFontId = -1;


    private PerformanceGraph? frameTimeGraph;
    private PerformanceGraph? cpuTimeGraph;
    private Stopwatch? frameTimer;
    private double previousFrameTimeSeconds;

    private Thread? renderLoopThread;
    private volatile bool isRenderLoopRunning;
    private volatile bool isDisposed;

    private uint surfacePixelWidth;
    private uint surfacePixelHeight;

    /// <summary>
    /// Gets the name of the currently active graphics backend for UI display.
    /// </summary>
    public string CurrentBackendName => _currentBackend.ToString();

    /// <summary>
    /// Initializes the GraphicsDevice from an Android Surface.
    /// Must be called from the UI thread when the surface is available (SurfaceCreated).
    /// </summary>
    public void InitializeFromSurface(Android.Views.Surface androidSurface, GraphicsBackend backend)
    {
        // Log which Veldrid assembly is loaded FIRST — before any other code that might throw
        var veldridAssembly = typeof(Veldrid.GraphicsDevice).Assembly;
        Log.Info(LOG_TAG, $"Veldrid assembly: {veldridAssembly.FullName}");
        Log.Info(LOG_TAG, $"Veldrid location: {veldridAssembly.Location}");
        var veldridBuildVersion = veldridAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        Log.Info(LOG_TAG, $"Veldrid build: {veldridBuildVersion ?? "(unknown)"}");

        try
        {
            Log.Info(LOG_TAG, $"InitializeFromSurface starting with backend: {backend}");

            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(VeldridAndroidRenderer),
                    "Cannot initialize — renderer has already been disposed. Create a new instance.");
            }

            _currentBackend = backend;

            // Get the Java Surface jobject handle — ppy.Veldrid expects the Java Surface reference,
            // NOT an ANativeWindow pointer. It handles the native window conversion internally.
            Log.Debug(LOG_TAG, "Getting Java Surface handle");
            IntPtr javaSurfaceHandle = androidSurface.Handle;
            if (javaSurfaceHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "Android Surface.Handle is null. The Surface may not be valid.");
            }
            Log.Debug(LOG_TAG, $"Java Surface handle: 0x{javaSurfaceHandle:X}");

            // Query surface dimensions via ANativeWindow (for NVG frame sizing)
            Log.Debug(LOG_TAG, "Getting surface dimensions via ANativeWindow");
            IntPtr nativeWindowForSizing = ANativeWindow_fromSurface(
                Java.Interop.JniEnvironment.EnvironmentPointer,
                javaSurfaceHandle);

            if (nativeWindowForSizing != IntPtr.Zero)
            {
                surfacePixelWidth = (uint)ANativeWindow_getWidth(nativeWindowForSizing);
                surfacePixelHeight = (uint)ANativeWindow_getHeight(nativeWindowForSizing);
                Log.Debug(LOG_TAG, $"ANativeWindow dimensions: {surfacePixelWidth}x{surfacePixelHeight}");
                ANativeWindow_release(nativeWindowForSizing);
            }
            else
            {
                Log.Warn(LOG_TAG, "ANativeWindow_fromSurface returned null, using default dimensions");
                surfacePixelWidth = 1280;
                surfacePixelHeight = 720;
            }
            Log.Info(LOG_TAG, $"Surface size: {surfacePixelWidth}x{surfacePixelHeight}");

            // Create GraphicsDevice with the user-selected backend — no automatic fallback.
            // If this fails, the exception propagates up and the user can try a different backend.
            Log.Info(LOG_TAG, $"Creating GraphicsDevice with {backend} backend");
            _graphicsDevice = CreateGraphicsDevice(androidSurface, backend);
            Log.Info(LOG_TAG, $"✓ GraphicsDevice created: {_graphicsDevice.BackendType}");

            // Create command list and NVG renderer
            Log.Debug(LOG_TAG, "Creating command list");
            renderCommandList = _graphicsDevice.ResourceFactory.CreateCommandList();

            Log.Debug(LOG_TAG, "Creating VeldridRenderer");
            nvgVeldridRenderer = new VeldridRenderer(_graphicsDevice);

            // Try simplified path first - skip NVG demo loading
            bool useSimplifiedPath = false; // Set to true for simplified testing

            if (useSimplifiedPath)
            {
                Log.Info(LOG_TAG, "Using simplified rendering path (skipping NVG demo)");

                // Skip NVG initialization, just create basic resources for clearing the screen
                Log.Debug(LOG_TAG, "Initializing VeldridRenderer (simplified path)");
                bool rendererCreated = nvgVeldridRenderer.Create();
                if (!rendererCreated)
                {
                    throw new InvalidOperationException("Failed to initialize VeldridRenderer");
                }

                Log.Debug(LOG_TAG, "Setting active command list");
                nvgVeldridRenderer.SetActiveCommandList(renderCommandList);

                // Skip NVG context and demo creation
                nvgContext = null;
                nvgDemo = null;
                frameTimeGraph = null;
                cpuTimeGraph = null;

                Log.Info(LOG_TAG, "Simplified initialization completed successfully");
            }
            else
            {
                Log.Debug(LOG_TAG, "Using full NVG rendering path");

                // Full initialization path
                Log.Debug(LOG_TAG, "Initializing VeldridRenderer");
                bool rendererCreated = nvgVeldridRenderer.Create();
                if (!rendererCreated)
                {
                    throw new InvalidOperationException("Failed to initialize VeldridRenderer");
                }

                Log.Debug(LOG_TAG, "Setting active command list");
                nvgVeldridRenderer.SetActiveCommandList(renderCommandList);

                Log.Debug(LOG_TAG, "Creating NVG context");
                nvgContext = Nvg.Create(nvgVeldridRenderer);

                // Load the shared NvgExample demo (fonts, images, UI widgets)
                // Demo.SetFileLoader() must have been called BEFORE this point (done in MainActivity.OnCreate)
                Log.Debug(LOG_TAG, "Creating NVG demo");
                nvgDemo = new Demo(nvgContext);

                // Look up the "sans" font registered by Demo for the backend label
                _backendLabelFontId = nvgContext.FindFont("sans");
                Log.Debug(LOG_TAG, $"Backend label font ID: {_backendLabelFontId}");

                Log.Debug(LOG_TAG, "Creating performance graphs");
                frameTimeGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Fps, "Frame Time");
                cpuTimeGraph = new PerformanceGraph(PerformanceGraph.GraphRenderStyle.Ms, "CPU Time");
            }

            frameTimer = Stopwatch.StartNew();
            previousFrameTimeSeconds = 0;

            Log.Info(LOG_TAG, "InitializeFromSurface completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(LOG_TAG, $"InitializeFromSurface failed with exception: {ex.GetType().Name}: {ex.Message}");
            Log.Error(LOG_TAG, $"Stack trace: {ex.StackTrace}");
            throw; // Re-throw to let the activity handle it
        }
    }

    /// <summary>
    /// Starts the render loop on a dedicated background thread.
    /// The touchMouseX/Y delegates provide the current touch position mapped to NVG coordinates.
    /// </summary>
    public void StartRenderLoop(Func<float> getTouchMouseX, Func<float> getTouchMouseY)
    {
        if (_graphicsDevice == null)
        {
            throw new InvalidOperationException(
                "Cannot start render loop — InitializeFromSurface() has not been called yet.");
        }

        isRenderLoopRunning = true;
        renderLoopThread = new Thread(() => RenderLoopBody(getTouchMouseX, getTouchMouseY))
        {
            Name = "NvgVeldridRenderThread",
            IsBackground = true,
            Priority = ThreadPriority.Highest  // Prioritize render thread
        };
        renderLoopThread.Start();
    }

    /// <summary>
    /// Notifies the renderer that the surface has been resized.
    /// Called from ISurfaceHolderCallback.SurfaceChanged on the UI thread.
    /// </summary>
    public void HandleSurfaceResize(uint newPixelWidth, uint newPixelHeight)
    {
        surfacePixelWidth = newPixelWidth;
        surfacePixelHeight = newPixelHeight;
        _graphicsDevice?.MainSwapchain?.Resize(newPixelWidth, newPixelHeight);
    }


    /// <summary>
    /// Stops the render loop and disposes all Veldrid/NVG resources.
    /// Safe to call multiple times.
    /// </summary>
    public void StopAndDispose()
    {
        if (isDisposed)
        {
            return;
        }
        isDisposed = true;
        isRenderLoopRunning = false;

        // Wait for render thread to finish
        renderLoopThread?.Join(TimeSpan.FromSeconds(2));

        _graphicsDevice?.WaitForIdle();

        nvgDemo?.Dispose();
        nvgContext?.Dispose();
        nvgVeldridRenderer?.Dispose();
        renderCommandList?.Dispose();
        _graphicsDevice?.Dispose();

        nvgDemo = null;
        nvgContext = null;
        nvgVeldridRenderer = null;
        renderCommandList = null;
        _graphicsDevice = null;
    }


    /// <summary>
    /// Creates a GraphicsDevice for the specified backend (Vulkan or OpenGL ES).
    /// For OpenGL ES, tries multiple depth/stencil formats in order of preference.
    /// </summary>
    private GraphicsDevice CreateGraphicsDevice(Android.Views.Surface androidSurface, GraphicsBackend backend)
    {
        try
        {
            Log.Info(LOG_TAG, $"CreateGraphicsDevice starting with backend: {backend}");

            IntPtr javaSurfaceHandle = androidSurface.Handle;
            IntPtr jniEnvironmentPointer = Java.Interop.JniEnvironment.EnvironmentPointer;

            if (backend == GraphicsBackend.Vulkan)
            {
                // Vulkan: use D24UNormS8UInt (widely supported)
                PixelFormat depthStencilFormat = VeldridCompat.DepthStencilD24S8;

                GraphicsDeviceOptions deviceOptions = new GraphicsDeviceOptions
                {
                    PreferDepthRangeZeroToOne = true,
                    SyncToVerticalBlank = false,
                    SwapchainDepthFormat = depthStencilFormat
                };

                Log.Info(LOG_TAG, $"Creating Vulkan backend: javaSurface=0x{javaSurfaceHandle:X}, jniEnv=0x{jniEnvironmentPointer:X}, depthFormat={depthStencilFormat}");

                SwapchainSource vulkanSwapchainSource = SwapchainSource.CreateAndroidSurface(
                    javaSurfaceHandle, jniEnvironmentPointer);

                SwapchainDescription vulkanSwapchainDesc = new SwapchainDescription(
                    vulkanSwapchainSource,
                    surfacePixelWidth,
                    surfacePixelHeight,
                    deviceOptions.SwapchainDepthFormat,
                    deviceOptions.SyncToVerticalBlank);

                return GraphicsDevice.CreateVulkan(deviceOptions, vulkanSwapchainDesc);
            }
            else if (backend == GraphicsBackend.OpenGLES)
            {
                // OpenGL ES: use D24UNormS8UInt (24-bit depth + 8-bit stencil).
                // EGL maps this to EGL_DEPTH_SIZE=24, EGL_STENCIL_SIZE=8 which is
                // universally supported on Android. D32FloatS8UInt (32-bit depth) is
                // NOT supported by most Android EGL implementations.
                // The stencil buffer is required for NVG's two-pass stencil fill rendering.
                PixelFormat glesDepthStencilFormat = VeldridCompat.DepthStencilD24S8;

                Log.Info(LOG_TAG, $"Creating OpenGL ES with depth format: {glesDepthStencilFormat}");

                GraphicsDeviceOptions deviceOptions = new GraphicsDeviceOptions
                {
                    PreferDepthRangeZeroToOne = true,
                    SyncToVerticalBlank = false,
                    SwapchainDepthFormat = glesDepthStencilFormat
                };

                SwapchainSource glesSwapchainSource = SwapchainSource.CreateAndroidSurface(
                    javaSurfaceHandle, jniEnvironmentPointer);

                SwapchainDescription glesSwapchainDesc = new SwapchainDescription(
                    glesSwapchainSource,
                    surfacePixelWidth,
                    surfacePixelHeight,
                    deviceOptions.SwapchainDepthFormat,
                    deviceOptions.SyncToVerticalBlank);

                return GraphicsDevice.CreateOpenGLES(deviceOptions, glesSwapchainDesc);
            }
            else
            {
                throw new NotSupportedException(
                    $"Graphics backend {backend} is not supported on Android. " +
                    $"Supported backends: Vulkan, OpenGLES");
            }
        }
        catch (Exception ex)
        {
            Log.Error(LOG_TAG, $"CreateGraphicsDevice failed with exception: {ex.GetType().Name}: {ex.Message}");
            Log.Error(LOG_TAG, $"Stack trace: {ex.StackTrace}");
            throw; // Re-throw to let the caller handle it
        }
    }

    private void RenderLoopBody(Func<float> getTouchMouseX, Func<float> getTouchMouseY)
    {
        Log.Info(LOG_TAG, "Render loop started");

        // Check if we're using simplified path (no NVG demo)
        bool isSimplifiedPath = nvgContext == null || nvgDemo == null ||
                               frameTimeGraph == null || cpuTimeGraph == null;

        if (isSimplifiedPath)
        {
            Log.Info(LOG_TAG, "Running simplified render loop (clear screen only)");
        }
        else
        {
            Log.Info(LOG_TAG, "Running full NVG demo render loop");
        }

        try
        {
            while (isRenderLoopRunning)
            {
                if (_graphicsDevice == null || renderCommandList == null)
                {
                    Log.Error(LOG_TAG, "Essential rendering resources are null, exiting render loop");
                    break;
                }

                if (!isSimplifiedPath && (nvgContext == null || nvgDemo == null ||
                    frameTimer == null || frameTimeGraph == null || cpuTimeGraph == null))
                {
                    Log.Error(LOG_TAG, "NVG resources are null, exiting render loop");
                    break;
                }

                try
                {
                    // Timing
                    double currentTimeSeconds = frameTimer?.Elapsed.TotalSeconds ?? 0;
                    double frameDeltaSeconds = currentTimeSeconds - previousFrameTimeSeconds;
                    previousFrameTimeSeconds = currentTimeSeconds;

                    // Read touch position (thread-safe via volatile reads in the delegates)
                    float currentTouchX = getTouchMouseX();
                    float currentTouchY = getTouchMouseY();

                    // Use desktop example dimensions (1000x600) for consistent layout
                    const float DESKTOP_DEMO_WIDTH = 1000f;
                    const float DESKTOP_DEMO_HEIGHT = 600f;
                    float nvgFrameWidth = DESKTOP_DEMO_WIDTH;
                    float nvgFrameHeight = DESKTOP_DEMO_HEIGHT;

                    // Calculate viewport to align left edge (not centered) to make room for toggle button
                    float scaleX = surfacePixelWidth / DESKTOP_DEMO_WIDTH;
                    float scaleY = surfacePixelHeight / DESKTOP_DEMO_HEIGHT;
                    float uniformScale = Math.Min(scaleX, scaleY);

                    uint scaledWidth = (uint)(DESKTOP_DEMO_WIDTH * uniformScale);
                    uint scaledHeight = (uint)(DESKTOP_DEMO_HEIGHT * uniformScale);
                    uint viewportX = 0; // Left-aligned instead of centered
                    uint viewportY = (surfacePixelHeight - scaledHeight) / 2; // Still vertically centered

                    // Begin GPU commands
                    renderCommandList.Begin();

                    // Only set framebuffer once to avoid potential double-clear issues
                    renderCommandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);

                    // Clear color based on backend type for visual distinction
                    RgbaFloat clearColor = _currentBackend == GraphicsBackend.Vulkan
                        ? new RgbaFloat(0.2f, 0.0f, 0.0f, 1.0f)  // Dark red for Vulkan
                        : new RgbaFloat(0.0f, 0.0f, 0.2f, 1.0f); // Dark blue for OpenGLES

                    // Clear entire framebuffer
                    renderCommandList.ClearColorTarget(0, clearColor);

                    // Only clear depth/stencil once
                    renderCommandList.ClearDepthStencil(0f, 0);

                    // Set viewport to the scaled content area (maintains aspect ratio, no stretching)
                    renderCommandList.SetViewport(0, new Viewport(viewportX, viewportY, scaledWidth, scaledHeight, 0f, 1f));
                    renderCommandList.SetScissorRect(0, viewportX, viewportY, scaledWidth, scaledHeight);

                    if (!isSimplifiedPath)
                    {
                        // Full NVG rendering path
                        // NVG frame — fully qualify SizeF to avoid ambiguity with Android.Util.SizeF
                        nvgContext!.BeginFrame(new System.Drawing.SizeF(nvgFrameWidth, nvgFrameHeight), 1.0f);

                        // Draw the full NanoVG demo (eyes, widgets, color wheel, graphs, etc.)
                        nvgDemo!.Render(currentTouchX, currentTouchY, nvgFrameWidth, nvgFrameHeight,
                            (float)currentTimeSeconds, blowup: false);

                        // Performance graphs
                        frameTimeGraph!.Render(5.0f, 5.0f, nvgContext);
                        cpuTimeGraph!.Render(5.0f + 200.0f + 5.0f, 5.0f, nvgContext);

                        // Backend label in lower-left corner
                        if (_backendLabelFontId >= 0)
                        {
                            nvgContext.Save();
                            nvgContext.FontFaceId(_backendLabelFontId);
                            nvgContext.FontSize(20.0f);
                            nvgContext.TextAlign(Align.Left | Align.Bottom);
                            nvgContext.FillColour(nvgContext.Rgba(255, 255, 255, 200));
                            nvgContext.Text(5.0f, nvgFrameHeight - 5.0f, $"Backend: {_currentBackend}");
                            nvgContext.Restore();
                        }

                        nvgContext.EndFrame();

                        double cpuTimeSeconds = frameTimer!.Elapsed.TotalSeconds - currentTimeSeconds;

                        frameTimeGraph.Update((float)frameDeltaSeconds);
                        cpuTimeGraph.Update((float)cpuTimeSeconds);
                    }

                    // Submit
                    renderCommandList.End();
                    _graphicsDevice.SubmitCommands(renderCommandList);
                    _graphicsDevice.SwapBuffers();

                    // Log first frame success
                    if (frameDeltaSeconds < 0.1)
                    {
                        Log.Info(LOG_TAG, $"✓ First frame rendered successfully with {_currentBackend} backend");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(LOG_TAG, $"Exception in render loop frame: {ex.GetType().Name}: {ex.Message}");
                    Log.Error(LOG_TAG, $"Stack trace: {ex.StackTrace}");
                    // Continue to next frame rather than breaking out of loop
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(LOG_TAG, $"Fatal exception in render loop: {ex.GetType().Name}: {ex.Message}");
            Log.Error(LOG_TAG, $"Stack trace: {ex.StackTrace}");
        }

        Log.Info(LOG_TAG, "Render loop stopped");
    }


    // --- Native Android window functions (libandroid.so) ---

    [DllImport("android")]
    private static extern IntPtr ANativeWindow_fromSurface(IntPtr jniEnvPointer, IntPtr javaSurfaceObject);

    [DllImport("android")]
    private static extern void ANativeWindow_release(IntPtr nativeWindowHandle);

    [DllImport("android")]
    private static extern int ANativeWindow_getWidth(IntPtr nativeWindowHandle);

    [DllImport("android")]
    private static extern int ANativeWindow_getHeight(IntPtr nativeWindowHandle);
}