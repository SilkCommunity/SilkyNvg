#nullable enable
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using System;
using System.IO;

namespace Veldrid_Android_Example;

/// <summary>
/// Android Activity that hosts a SurfaceView for Veldrid rendering.
/// Implements ISurfaceHolderCallback to manage the native surface lifecycle,
/// and maps touch events to mouse coordinates for the NanoVG demo.
/// </summary>
[Activity(
    Name = "com.arcane.veldrid.android.example.MainActivity",
    Label = "SilkyNvg Veldrid",
    MainLauncher = true,
    ScreenOrientation = ScreenOrientation.Landscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.KeyboardHidden,
    Theme = "@android:style/Theme.NoTitleBar.Fullscreen")]
public class MainActivity : Activity, ISurfaceHolderCallback
{
    private SurfaceView renderSurfaceView = null!;
    private VeldridAndroidRenderer veldridRenderer = null!;

    // Touch position mapped to NVG "mouse" coordinates
    private float touchMouseX;
    private float touchMouseY;

    // Backend chosen by the user via the selection dialog
    private Veldrid.GraphicsBackend? userSelectedBackend;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Always show the backend selection dialog — user must choose explicitly
        ShowBackendSelectionDialog();
    }

    private void ShowBackendSelectionDialog()
    {
        var builder = new Android.App.AlertDialog.Builder(this);
        builder.SetTitle("Select Graphics Backend");
        builder.SetMessage("Choose which graphics API to use:");

        builder.SetPositiveButton("Vulkan", (sender, args) =>
        {
            userSelectedBackend = Veldrid.GraphicsBackend.Vulkan;
            Android.Util.Log.Info("NvgVeldrid", "User selected Vulkan backend");
            InitializeRenderer();
        });

        builder.SetNegativeButton("OpenGL ES", (sender, args) =>
        {
            userSelectedBackend = Veldrid.GraphicsBackend.OpenGLES;
            Android.Util.Log.Info("NvgVeldrid", "User selected OpenGL ES backend");
            InitializeRenderer();
        });

        builder.SetCancelable(false); // Must select a backend
        builder.Show();
    }

    private void InitializeRenderer()
    {
        // Wire up Demo's file loader to read from Android AssetManager BEFORE any Demo construction
        NvgExample.Demo.SetFileLoader(LoadAssetFileBytes);

        // Create a SurfaceView for Veldrid to render into
        renderSurfaceView = new SurfaceView(this);
        renderSurfaceView.Holder!.AddCallback(this);

        // Let system bars (status bar and navigation bar) show, but fit content around them
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            Window?.SetDecorFitsSystemWindows(true); // Content fits within system bars
        }

        SetContentView(renderSurfaceView);

        veldridRenderer = new VeldridAndroidRenderer();
    }

    /// <summary>
    /// Loads a file from Android's AssetManager as a byte array.
    /// The Demo class uses paths like "./fonts/Roboto-Regular.ttf" and "./images/image0.jpg".
    /// We strip the leading "./" to get the AssetManager-relative path.
    /// </summary>
    private byte[] LoadAssetFileBytes(string relativeFilePath)
    {
        // Strip leading "./" that Demo.cs uses for desktop-relative paths
        string assetPath = relativeFilePath;
        if (assetPath.StartsWith("./", StringComparison.Ordinal))
        {
            assetPath = assetPath.Substring(2);
        }

        using Stream assetStream = Assets!.Open(assetPath);
        using MemoryStream memoryBuffer = new MemoryStream();
        assetStream.CopyTo(memoryBuffer);
        return memoryBuffer.ToArray();
    }

    // --- ISurfaceHolderCallback ---

    public void SurfaceCreated(ISurfaceHolder surfaceHolder)
    {
        const string LOG_TAG = "NvgVeldrid";
        try
        {
            Android.Util.Log.Info(LOG_TAG, "SurfaceCreated — initializing Veldrid GraphicsDevice");

            if (surfaceHolder.Surface == null)
            {
                Android.Util.Log.Error(LOG_TAG, "SurfaceCreated called with null Surface!");
                return;
            }

            Android.Util.Log.Debug(LOG_TAG, $"Surface valid: {surfaceHolder.Surface.IsValid}");
            Android.Util.Log.Debug(LOG_TAG, $"Surface handle: 0x{surfaceHolder.Surface.Handle:X}");

            // Create a fresh renderer instance each time the surface is (re)created.
            // The previous instance was disposed in SurfaceDestroyed, so we cannot reuse it.
            veldridRenderer = new VeldridAndroidRenderer();

            Veldrid.GraphicsBackend backendToUse = userSelectedBackend!.Value;
            Android.Util.Log.Info(LOG_TAG, $"Backend to use: {backendToUse}");

            Android.Util.Log.Info(LOG_TAG, $"Initializing with {backendToUse} backend");
            veldridRenderer.InitializeFromSurface(surfaceHolder.Surface, backendToUse);

            Android.Util.Log.Info(LOG_TAG, "Starting render loop");
            veldridRenderer.StartRenderLoop(
                () => touchMouseX,
                () => touchMouseY);
        }
        catch (Exception ex)
        {
            Android.Util.Log.Error(LOG_TAG, $"SurfaceCreated failed: {ex.GetType().Name}: {ex.Message}");
            Android.Util.Log.Error(LOG_TAG, $"Stack trace: {ex.StackTrace}");

            // Determine which backend the user could try instead
            Veldrid.GraphicsBackend failedBackend = userSelectedBackend!.Value;
            Veldrid.GraphicsBackend alternateBackend = failedBackend == Veldrid.GraphicsBackend.Vulkan
                ? Veldrid.GraphicsBackend.OpenGLES
                : Veldrid.GraphicsBackend.Vulkan;

            RunOnUiThread(() =>
            {
                var errorDialogBuilder = new Android.App.AlertDialog.Builder(this);
                errorDialogBuilder.SetTitle($"{failedBackend} Initialization Failed");
                errorDialogBuilder.SetMessage($"{ex.GetType().Name}: {ex.Message}");
                errorDialogBuilder.SetPositiveButton($"Try {alternateBackend}", (sender, args) => {
                    userSelectedBackend = alternateBackend;
                    // Re-create the surface view to trigger SurfaceCreated with the new backend
                    InitializeRenderer();
                });
                errorDialogBuilder.SetNegativeButton("Exit", (sender, args) => Finish());
                errorDialogBuilder.SetCancelable(false);
                errorDialogBuilder.Show();
            });
        }
    }

    public void SurfaceChanged(ISurfaceHolder surfaceHolder, Android.Graphics.Format format, int width, int height)
    {
        Android.Util.Log.Info("NvgVeldrid", $"SurfaceChanged: {width}x{height} format={format}");
        veldridRenderer.HandleSurfaceResize((uint)width, (uint)height);
    }

    public void SurfaceDestroyed(ISurfaceHolder surfaceHolder)
    {
        Android.Util.Log.Info("NvgVeldrid", "SurfaceDestroyed — stopping render loop and disposing");
        veldridRenderer.StopAndDispose();
    }

    // --- Touch → Mouse mapping ---

    public override bool OnTouchEvent(MotionEvent? touchEvent)
    {
        if (touchEvent == null)
        {
            return base.OnTouchEvent(touchEvent);
        }

        float touchX = touchEvent.GetX();
        float touchY = touchEvent.GetY();

        switch (touchEvent.Action)
        {
            case MotionEventActions.Down:
            case MotionEventActions.Move:
                touchMouseX = touchX;
                touchMouseY = touchY;
                break;
        }

        return true;
    }

    protected override void OnPause()
    {
        // Exit the app when the user navigates away (home button, task switcher, etc.)
        // This is a rendering demo — no reason to keep it alive in the background.
        Android.Util.Log.Info("NvgVeldrid", "OnPause — finishing activity");
        Finish();
        base.OnPause();
    }

    protected override void OnDestroy()
    {
        veldridRenderer?.StopAndDispose();
        base.OnDestroy();
    }
}