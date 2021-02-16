using Silk.NET.Maths;
using SilkyNvg.Core;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.OpenGL;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        private readonly GraphicsManager _graphicsManager;
        private readonly InstructionManager _instructionManager;
        private readonly StateManager _stateManager;
        private readonly PathCache _pathCache;
        private readonly Style _style;

        private Nvg(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            // TODO: Images
            _instructionManager = new InstructionManager();
            _pathCache = new PathCache();
            _stateManager = new StateManager();
            _style = new Style(1.0f);
            _graphicsManager.CreateRenderer();
            // TODO: More images
        }

        /// <summary>
        /// Save the current render state into the state stack.
        /// </summary>
        public void Save()
        {
            _stateManager.Save();
        }

        /// <summary>
        /// Pops the state stack.
        /// </summary>
        public void Restore()
        {
            _stateManager.Restore();
        }

        /// <summary>
        /// Resets the current render state. Renderstack
        /// is kept.
        /// </summary>
        public void Reset()
        {
            _stateManager.Reset();
        }

        /// <summary>
        /// Transform a point by the specified transform.
        /// </summary>
        /// <param name="x">The point x position.</param>
        /// <param name="y">The point y position.</param>
        /// <param name="t">The transform.</param>
        /// <returns>The transformed point.</returns>
        public Vector2D<float> TransformPoint(float x, float y, params float[] t)
        {
            return Maths.TransformPoint(x, y, t);
        }

        /// <summary>
        /// Transform a point by the specified transform.
        /// </summary>
        /// <param name="pos">The point</param>
        /// <param name="t">The transform</param>
        /// <returns>The transformed point.</returns>
        public Vector2D<float> TransformPoint(Vector2D<float> pos, params float[] t)
        {
            return TransformPoint(pos.X, pos.Y, t);
        }

        /// <summary>
        /// Creates a new instance of the NanoVG API. Each Nvg-API has its own
        /// context, so instead of needing to parse it in all the time, one creates
        /// a new API-Instance. An API instance will also be refered to as "context"
        /// in the following documentation, when it is more applicable.
        /// </summary>
        /// <param name="flags">The flags to be used when this context is running <see cref="CreateFlag"/>.</param>
        /// <param name="gl">The GL Api object needed for rendering.</param>
        public static Nvg Create(uint flags, Silk.NET.OpenGL.GL gl)
        {
            var launchParams = new LaunchParameters((flags & (uint)CreateFlag.EdgeAntialias) != 0,
                (flags & (uint)CreateFlag.StencilStrokes) != 0, (flags & (uint)CreateFlag.Debug) != 0);
            var gManager = new GraphicsManager(launchParams, gl);
            var nvg = new Nvg(gManager);
            return nvg;
        }

    }
}
