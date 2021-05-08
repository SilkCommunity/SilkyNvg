using SilkyNvg.Common;
using SilkyNvg.Core;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;

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
            instructionQueue = new InstructionQueue();

            // Path Cache
            pathCache = new PathCache();

            // States
            stateStack = new StateStack();
            Save();
            Reset();

            // Pixel Ratio
            pixelRatio = new();
            pixelRatio.DeviePxRatio = 1.0f;

            if (!graphicsManager.Create())
            {
                throw new System.Exception("Failed to initialize the renderer!");
            }

            // TODO: More on text N stuff
        }

        #region States
        public void Save()
        {
            stateStack.Save();
        }

        public void Reset()
        {
            stateStack.Restore();
        }
        #endregion

    }
}
