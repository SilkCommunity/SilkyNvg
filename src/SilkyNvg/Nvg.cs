using Silk.NET.Maths;
using SilkyNvg.Core;
using SilkyNvg.Instructions;
using SilkyNvg.Paths;
using SilkyNvg.OpenGL;
using SilkyNvg.States;

namespace SilkyNvg
{
    public sealed class Nvg
    {

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
            _graphicsManager.CreateRenderer();
            // TODO: Font
            // TODO: More images

            _frameMeta = new FrameMeta();
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
            _stateManager.Reset();
            _style.Update(pixelRatio);
            _graphicsManager.SetViewport(windowWidth, windowHeight);
            _frameMeta.Reset();
        }

        public void EndFrame()
        {

        }

        /// <summary>
        /// Create a new Colour with a vector.
        /// All colours are represented between 0 and 1
        /// where 0 is 0 and 1 is 255.
        /// </summary>
        /// <param name="r">Red component</param>
        /// <param name="g">Green component</param>
        /// <param name="b">Blue component</param>
        /// <param name="a">Alpha component</param>
        /// <returns>A new colour with the specified values.</returns>
        public Colour FromRGBAf(float r, float g, float b, float a)
        {
            return Colour.FromRGBAf(r, g, b, a);
        }

        /// <summary>
        /// Transform a point by the specified transform.
        /// </summary>
        /// <param name="pos">The point</param>
        /// <param name="t">The transform</param>
        /// <returns>The transformed point.</returns>
        public Vector2D<float> TransformPoint(Vector2D<float> pos, Matrix3X2<float> transform)
        {
            return Maths.TransformPoint(pos, transform);
        }

        /// <summary>
        /// NVG uses 3x2 matrices.
        /// </summary>
        /// <returns>A 3x2 identity matrix.</returns>
        public Matrix3X2<float> TransformIdentity()
        {
            return Maths.TransformIdentity;
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
        /// Set the colour to be used when
        /// calling <see cref="Fill"/>
        /// </summary>
        /// <param name="colour">The new colour. <see cref="FromRGBAf(float, float, float, float)"/></param>
        public void FillColour(Colour colour)
        {
            var state = _stateManager.GetCurrentState();
            state.Fill = new Paint(Maths.TransformIdentity, 0.0f, 1.0f, colour, colour);
        }

        /// <summary>
        /// Clear the current path and sub-path.
        /// </summary>
        public void BeginPath()
        {
            _instructionManager.Clear();
            _pathCache.Clear();
        }

        /// <summary>
        /// Draw an ellipse with the specified parameters.
        /// </summary>
        /// <param name="position">The centre of the ellipse.</param>
        /// <param name="radiusX">The x-axis radius of the ellipse.</param>
        /// <param name="radiusY">The y-axis radius of the ellipse.</param>
        public void Ellipse(Vector2D<float> position, float radiusX, float radiusY)
        {
            float x = position.X;
            float y = position.Y;
            var sequence = new InstructionSequence(6, _stateManager.GetCurrentState());
            sequence.AddMoveTo(x - radiusX, position.Y);
            sequence.AddBezireTo(
                x - radiusX,
                y + radiusY * Maths.Kappa,
                x - radiusX * Maths.Kappa,
                y + radiusY,
                x,
                y + radiusY
            );
            sequence.AddBezireTo(
                x + radiusX * Maths.Kappa,
                y + radiusY,
                x + radiusX,
                y + radiusY * Maths.Kappa,
                x + radiusX,
                y
            );
            sequence.AddBezireTo(
                x + radiusX,
                y - radiusY * Maths.Kappa,
                x + radiusX * Maths.Kappa,
                y - radiusY,
                x,
                y - radiusY
            );
            sequence.AddBezireTo(
                x - radiusX * Maths.Kappa,
                y - radiusY,
                x - radiusX,
                y - radiusY * Maths.Kappa,
                x - radiusX,
                y
            );
            sequence.AddClose();
            _instructionManager.AddSequence(sequence);
        }

        /// <summary>
        /// Draw a "quadratic ellipse".
        /// Some like to call them circles.
        /// </summary>
        /// <param name="position">The circle's centre.</param>
        /// <param name="radius">The circle's radius. (X- and Y- axis!)</param>
        public void Circle(Vector2D<float> position, float radius)
        {
            Ellipse(position, radius, radius);
        }

        /// <summary>
        /// Apply fill. I.e. fill the path specified before with the
        /// set fill colour. <see cref="FillColour(Colour)"/>
        /// </summary>
        public void Fill()
        {
            var state = _stateManager.GetCurrentState();
            var fillPaint = state.Fill;

            _pathCache.FlattenPaths(_instructionManager, _style);
            if (_graphicsManager.LaunchParameters.EdgeAntialias && state.ShapeAntiAlias)
            {

            }

        }

    }
}
