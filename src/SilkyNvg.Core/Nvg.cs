using SilkyNvg.Common;
using SilkyNvg.Core;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using System.Numerics;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        public static Nvg Create(CreateFlags flags, INvgRenderer renderer)
        {
            LaunchParameters launchParameters = new(
                (flags & CreateFlags.Antialias) != 0,
                (flags & CreateFlags.StencilStrokes) != 0,
                (flags & CreateFlags.Debug) != 0
            );

            GraphicsManager graphicsManager = new(launchParameters, renderer);

            Nvg nvg = new(launchParameters, graphicsManager);
            return nvg;
        }

        #region Meta

        public FrameMeta FrameMeta { get; internal set; }

        #endregion

        #region Implementation
        internal readonly LaunchParameters launchParameters;
        internal readonly GraphicsManager graphicsManager;

        internal readonly InstructionQueue instructionQueue;
        internal readonly PathCache pathCache;
        internal readonly StateStack stateStack;

        internal PixelRatio pixelRatio;

        private Nvg(LaunchParameters launchParameters, GraphicsManager graphicsManager)
        {
            this.launchParameters = launchParameters;
            this.graphicsManager = graphicsManager;

            // TODO: Font images

            // Instructions
            instructionQueue = new();

            // Path Cache
            pathCache = new();

            // States
            stateStack = new();
            stateStack.Save();
            stateStack.Reset();

            // Pixel Ratio
            pixelRatio = new();
            pixelRatio.DevicePxRatio = 1.0f;

            if (!graphicsManager.Create())
            {
                throw new System.Exception("Failed to initialize the renderer!");
            }

            // TODO: More on text N stuff
        }
        #endregion

        #region Frames
        public void BeginFrame(Vector2 viewSize, float devicePxRatio)
        {
            stateStack.Clear();
            stateStack.Save();
            stateStack.Reset();

            pixelRatio.DevicePxRatio = devicePxRatio;

            graphicsManager.Viewport(viewSize, devicePxRatio);

            FrameMeta = default;
        }

        public void BeginFrame(float x, float y, float devicePxRatioo) => BeginFrame(new(x, y), devicePxRatioo);
        #endregion

        #region State Handling
        public void Save()
        {
            stateStack.Save();
        }

        public void Reset()
        {
            stateStack.Restore();
        }

        public void Restore()
        {
            stateStack.Restore();
        }
        #endregion

        #region State Handling
        public void StrokeColour(Colour colour)
        {
            StrokePaint(new Paint(colour));
        }

        public void StrokePaint(Paint paint)
        {
            stateStack.CurrentState.stroke = paint;
        }

        public void FillColour(Colour colour)
        {
            FillPaint(new Paint(colour));
        }

        public void FillPaint(Paint paint)
        {
            stateStack.CurrentState.fill = paint;
        }
        #endregion

    }
}
