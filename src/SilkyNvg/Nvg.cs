namespace SilkyNvg
{
    public partial class Nvg
    {

        /// <summary>
        /// Create a new Nvg instance.
        /// </summary>
        /// <param name="createFlags">The flags specific to this context. <see cref="CreateFlags"/></param>
        /// <returns>A new API instance of Nvg</returns>
        public static Nvg Create(uint createFlags)
        {
            return null;
        }

        private Nvg()
        {

        }

        /// <summary>
        /// Delete this Nvg instance and
        /// free the OpenGL resources from
        /// memory.
        /// </summary>
        public void Delete()
        {

        }

        /// <summary>
        /// Begins rendering a new frame.
        /// Calls to Nvg should be wrapped in this and
        /// <see cref="EndFrame"/>
        /// </summary>
        /// <param name="windowWidth">The window's width</param>
        /// <param name="windowHeight">The window's height</param>
        /// <param name="pixelRatio">The pixel ratio, computed as
        /// <code>frameBufferWidth / windowWidth</code></param>
        public void BeginFrame(float windowWidth, float windowHeight, float pixelRatio)
        {

        }

        // TODO: Cancel frame

        /// <summary>
        /// Finish drawing a frame and
        /// render all queued up objects.
        /// All calls to Nvg should be wrapped in
        /// <see cref="BeginFrame(float, float, float)"/> and this.
        /// </summary>
        public void EndFrame()
        {

        }

        // TODO: Composite Operations

        /// <summary>
        /// Create a <see cref="Colour"/> from the rgb components
        /// between 0 and 255. Alpha will be 255.
        /// </summary>
        /// <param name="r">The red component (0 - 255)</param>
        /// <param name="g">The green component (0 - 255)</param>
        /// <param name="b">The blue component (0 - 255)</param>
        /// <returns>A new Colour</returns>
        public Colour Rgb(byte r, byte g, byte b)
        {
            return new Colour(r, g, b);
        }

        /// <summary>
        /// Create a <see cref="Colour"/> from the rgb components
        /// between 0 and 1. Alpha will be 1.
        /// </summary>
        /// <param name="r">The red component (0 - 1)</param>
        /// <param name="g">The green component (0 - 1)</param>
        /// <param name="b">The blue component (0 - 1)</param>
        /// <returns>A new Colour</returns>
        public Colour RgbF(float r, float g, float b)
        {
            return new Colour(r, g, b);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> from the rgba components
        /// between 0 and 255.
        /// </summary>
        /// <param name="r">The red component (0 - 255)</param>
        /// <param name="g">The green component (0 - 255)</param>
        /// <param name="b">The blue component (0 - 255)</param>
        /// <param name="a">The alpha component (0 - 255)</param>
        /// <returns>A new Colour</returns>
        public Colour Rgba(byte r, byte g, byte b, byte a)
        {
            return new Colour(r, g, b, a);
        }

        /// <summary>
        /// Create a new <see cref="Colour"/> from the rgba components
        /// between 0 and 1.
        /// </summary>
        /// <param name="r">The red component (0 - 1)</param>
        /// <param name="g">The green component (0 - 1)</param>
        /// <param name="b">The blue component (0 - 1)</param>
        /// <param name="a">The alpha component (0 - 1)</param>
        /// <returns>A new Colour</returns>
        public Colour RgbaF(float r, float g, float b, float a)
        {
            return new Colour(r, g, b, a);
        }

        // TODO: More Colour

        /// <summary>
        /// Save the current render state to the
        /// state stack by pushing a copy of this state
        /// beneath it.
        /// Restore by <see cref="Restore"/>
        /// </summary>
        public void Save()
        {

        }

        /// <summary>
        /// Restores the last saved render state.
        /// To restore further back, call the
        /// adequate ammount.
        /// </summary>
        public void Restore()
        {

        }

        /// <summary>
        /// Reset the current state to default values
        /// but keeps the state stack.
        /// </summary>
        public void Reset()
        {

        }

        // TODO: Render styles
        /// <summary>
        /// Set the current fill paint to
        /// this colour.
        /// </summary>
        /// <param name="colour">The new colour of the fill paint.</param>
        public void FillColour(Colour colour)
        {

        }

        // TODO: Transforms and Maths

        // TODO: Image

        // TODO: Paint

        // TODO: Scissor

        /// <summary>
        /// Prepares Nvg for a new path.
        /// Call before defining a new path.
        /// </summary>
        public void BeginPath()
        {

        }

        // TODO: Paths

        // TODO: Shapes
        /// <summary>
        /// Render specified rectangle at specified
        /// location. Adds rectangle shape to path.
        /// </summary>
        /// <param name="x">The X-Position of the rectangle</param>
        /// <param name="y">The Y-Position of the rectangle</param>
        /// <param name="w">The width of the rectangle</param>
        /// <param name="h">The height of the rectangle</param>
        public void Rect(float x, float y, float w, float h)
        {

        }

        /// <summary>
        /// Fills the current path
        /// with the current fill Paint / Colour.
        /// <see cref="FillColour(Colour)"/>
        /// </summary>
        public void Fill()
        {

        }

        // TODO: Stroke

        // TODO: Font

    }
}
