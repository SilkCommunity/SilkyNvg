using SilkyNvg.Base;
using SilkyNvg.Colouring;
using SilkyNvg.Common;
using SilkyNvg.Core;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.OpenGL;
using SilkyNvg.Paths;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        #region Initilization
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

        private readonly GraphicsManager _graphicsManager;
        private readonly InstructionManager _instructionManager;
        private readonly StateManager _stateManager;
        private readonly PathCache _pathCache;
        private readonly Style _style;

        private readonly FrameMeta _frameMeta;

        private Nvg(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            // TODO: Images
            _instructionManager = new InstructionManager();
            _pathCache = new PathCache();
            _stateManager = new StateManager();
            _style = new Style(1.0f);
            _graphicsManager.Create();
            // TODO: Font
            // TODO: More images

            _frameMeta = new FrameMeta();
        }
        #endregion

        #region Frames
        /// <summary>
        /// Begins rendering a new frame.
        /// All calls to NanoVG per frame should be wrapped in
        /// calls of this method and <see cref="EndFrame"/>.
        /// </summary>
        /// <param name="windowWidth">The width of the window
        /// (or the portion of the window)</param>
        /// <param name="windowHeight">The height of the window
        /// (or the portion of the window)</param>
        /// <param name="pixelRatio">The ratio of the frame buffer
        /// size to the window size, computed as <code>pixelRatio = frameBufferWidth / windowWidth</code></param>
        public void BeginFrame(float windowWidth, float windowHeight, float pixelRatio)
        {
            _stateManager.ClearStack();
            _stateManager.Save();
            _stateManager.Reset();
            _style.CalculateForPixelRatio(pixelRatio);
            _graphicsManager.Viewport(windowWidth, windowHeight);
            _frameMeta.Reset();
        }

        /// <summary>
        /// Ends rendering a new frame.
        /// All call to NanoVG per frame should be wrapped in
        /// calls of <see cref="BeginFrame(float, float, float)"/> and this.
        /// </summary>
        public void EndFrame()
        {
            _graphicsManager.Flush();
            _pathCache.ClearVerts();
        }
        #endregion

        #region Colours
        /// <summary>
        /// Create a new colour using the following
        /// RGBA-Parameters, specified as parts of float.
        /// I.E. devide the 0-255 by 255.
        /// <seealso cref="Colour.Colour(float, float, float, float)"/>
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        /// <returns>A colour using the specified rgba values.</returns>
        public Colour RGBAf(float r, float g, float b, float a)
        {
            return new Colour(r, g, b, a);
        }
        #endregion

        #region States
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
        #endregion

        #region RenderStyles
        /// <summary>
        /// Set the colour to be used when calling
        /// <see cref="Fill"/>.
        /// </summary>
        /// <param name="colour">The colour the path is to be filled with.</param>
        public void FillColour(Colour colour)
        {
            var state = _stateManager.GetState();
            state.Fill = new Paint(colour);
        }
        #endregion

        #region Paths
        /// <summary>
        /// Clear the current path and sub-path.
        /// </summary>
        public void BeginPath()
        {
            _instructionManager.Clear();
            _pathCache.Clear();
        }

        /// <summary>
        /// Draw a rectangle at give position
        /// with given size.
        /// </summary>
        /// <param name="x">The X Position</param>
        /// <param name="y">The Y Position</param>
        /// <param name="w">The width</param>
        /// <param name="h">The height</param>
        public void Rect(float x, float y, float w, float h)
        {
            var sequence = new InstructionSequence(5);
            sequence.AddMoveTo(x, y);
            sequence.AddLineTo(x, y + h);
            sequence.AddLineTo(x + w, y + h);
            sequence.AddLineTo(x + w, y);
            sequence.AddClose();
            _instructionManager.AddSequence(sequence, _stateManager.GetState());
        }

        /// <summary>
        /// Fill the current path using
        /// the set fill colour <see cref="FillColour(Colour)"/>
        /// </summary>
        public void Fill()
        {
            var state = _stateManager.GetState();
            var fillPaint = state.Fill;

            _pathCache.FlattenPaths(_instructionManager, _style);
            if (_graphicsManager.LaunchParameters.Antialias && state.ShapeAntiAlias)
            {
                _pathCache.ExpandFill(_style.FringeWidth, LineCap.Miter, 2.4f, _style);
            }
            else
            {
                _pathCache.ExpandFill(0.0f, LineCap.Miter, 2.4f, _style);
            }

            var inner = fillPaint.InnerColour;
            var outer = fillPaint.OuterColour;
            inner.A *= state.Alpha;
            outer.A *= state.Alpha;

            _graphicsManager.Fill(fillPaint, state.CompositeOperation, state.Scissor, _style.FringeWidth, _pathCache.Bounds, _pathCache.Paths);

            for (int i = 0; i < _pathCache.Paths.Count; i++)
            {
                var path = _pathCache.Paths[i];
                _frameMeta.FillTriCount += path.Fill.Count - 2;
                _frameMeta.FillTriCount += path.Stroke.Count - 2;
                _frameMeta.DrawCallCount += 2;
            }
        }
        #endregion

    }
}