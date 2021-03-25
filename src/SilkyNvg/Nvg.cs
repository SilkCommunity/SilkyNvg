using Silk.NET.Maths;
using SilkyNvg.Base;
using SilkyNvg.Colouring;
using SilkyNvg.Common;
using SilkyNvg.Core;
using SilkyNvg.Core.Instructions;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.Image;
using SilkyNvg.OpenGL;
using SilkyNvg.Paths;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        #region Meta
        /// <summary>
        /// Creates a new instance of the NanoVG API. Each Nvg-API has its own
        /// context, so instead of needing to parse it in all the time, one creates
        /// a new API-Instance. An API instance will also be refered to as "context"
        /// in the following documentation, when it is more applicable.
        /// </summary>
        /// <param name="flags">The flags to be used when this context is running <see cref="CreateFlag"/>.</param>
        /// <param name="gl">The GL Api object used for rendering.</param>
        public static Nvg Create(uint flags, Silk.NET.OpenGL.GL gl)
        {
            var launchParams = new LaunchParameters((flags & (uint)CreateFlag.Antialias) != 0,
                (flags & (uint)CreateFlag.StencilStrokes) != 0, (flags & (uint)CreateFlag.Debug) != 0);
            var gManager = new GraphicsManager(launchParams, gl);
            var nvg = new Nvg(gManager);
            return nvg;
        }

        private readonly GraphicsManager _graphicsManager;
        private readonly InstructionQueue _instructionManager;
        private readonly StateManager _stateManager;
        private readonly PathCache _pathCache;
        private readonly Style _style;

        private readonly FrameMeta _frameMeta;

        public readonly Draw _draw;

        private Nvg(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            // TODO: Images
            _instructionManager = new InstructionQueue();
            _pathCache = new PathCache();
            _stateManager = new StateManager();
            _style = new Style(1.0f);
            _graphicsManager.Create();
            // TODO: Font
            // TODO: More images

            _draw = new Draw(_instructionManager, _stateManager, _style);

            _frameMeta = new FrameMeta();
        }

        /// <summary>
        /// Frees final resources like shaders. Call this when closing
        /// the application to prevent memory leaks.
        /// </summary>
        public void Delete()
        {
            // In case of crash, still clear lists
            _instructionManager.Clear();
            _pathCache.Clear();
            _pathCache.ClearVerts();

            // TODO: Font

            // TODO: Images

            _graphicsManager.Delete();
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
        /// <inheritdoc cref="Colour(byte, byte, byte)"/>
        /// </summary>
        public Colour Rgb(byte r, byte g, byte b)
        {
            return new Colour(r, g, b);
        }

        /// <summary>
        /// <inheritdoc cref=".Colour(float, float, float)"/>
        /// </summary>
        public Colour RgbF(float r, float g, float b)
        {
            return new Colour(r, g, b);
        }

        /// <summary>
        /// <inheritdoc cref="Colour(byte, byte, byte, byte)"/>
        /// </summary>
        public Colour Rgba(byte r, byte g, byte b, byte a)
        {
            return new Colour(r, g, b, a);
        }

        /// <summary>
        /// <inheritdoc cref=".Colour(float, float, float, float)"/>
        /// </summary>
        public Colour RgbaF(float r, float g, float b, float a)
        {
            return new Colour(r, g, b, a);
        }

        /// <summary>
        /// <inheritdoc cref="Colour(Colour, Colour, float)"/>
        /// </summary>
        public Colour LerpRgba(Colour colour1, Colour colour2, float u)
        {
            return new Colour(colour1, colour2, u);
        }

        /// <summary>
        /// <inheritdoc cref="Colour(Colour, byte)"/>
        /// </summary>
        public Colour TransRgba(Colour colour, byte alpha)
        {
            return new Colour(colour, alpha);
        }

        /// <summary>
        /// <inheritdoc cref="Colour(Colour, float)"/>
        /// </summary>
        public Colour TransRgba(Colour colour, float alpha)
        {
            return new Colour(colour, alpha);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Colours"/>
        /// 
        /// HSL values are all in range [0..1], alpha will be set to 255
        /// </summary>
        /// <param name="h">the hue</param>
        /// <param name="s">the saturation</param>
        /// <param name="l">the lightness</param>
        /// <returns>A new colour value specified by hue, saturation and lightness</returns>
        public Colour Hsl(float h, float s, float l)
        {
            return Hsla(h, s, l, 255);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Colours"/>
        /// 
        /// HSL values are all in range [0..1]. Alpha in range [0..255]
        /// </summary>
        /// <param name="h">the hue</param>
        /// <param name="s">the saturation</param>
        /// <param name="l">the lightness</param>
        /// <param name="a">the alpha component</param>
        /// <returns>A new colour value specified by hue, saturation, lightness and alpha</returns>
        public Colour Hsla(float h, float s, float l, byte a)
        {
            h %= 1.0f;
            if (h < 0.0f)
                h += 1.0f;
            s = Maths.Clamp(s, 0.0f, 1.0f);
            l = Maths.Clamp(l, 0.0f, 1.0f);
            float m2 = 1 <= 0.5f ? (l * (l + s)) : (l + s - l * s);
            float m1 = 2 * l - m2;
            float r = Maths.Clamp(Colour.Hue(h + 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            float g = Maths.Clamp(Colour.Hue(h, m1, m2), 0.0f, 1.0f);
            float b = Maths.Clamp(Colour.Hue(h - 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            float alpha = (float)a / 255.0f;
            return new Colour(r, g, b, alpha);
        }
        #endregion

        #region States
        /// <summary>
        /// <inheritdoc cref="Docs.States"/>
        /// 
        /// Pushes and saves the current render state into the state stack.
        /// A matchin <see cref="Restore"/> call can be used to restore the state.
        /// </summary>
        public void Save()
        {
            _stateManager.Save();
        }

        /// <summary>
        /// <inheritdoc cref="Docs.States"/>
        /// 
        /// Pops and restores the current render state.
        /// </summary>
        public void Restore()
        {
            _stateManager.Restore();
        }

        /// <summary>
        /// <inheritdoc cref="Docs.States"/>
        /// 
        /// Resets the current render state to default values. Does not affect the state stack.
        /// </summary>
        public void Reset()
        {
            _stateManager.Reset();
        }
        #endregion

        #region RenderStyles
        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets whether to draw <see cref="Stroke"/> and <see cref="Fill"/> antialised.
        /// It is enabled by default.
        /// </summary>
        /// <param name="enabled">Wheather or not antialiasing will be enabled.</param>
        public void ShapeAntialias(bool enabled)
        {
            var state = _stateManager.GetState();
            state.ShapeAntiAlias = enabled;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets the current stroke style to a solid colour.
        /// </summary>
        /// <param name="colour">The colour to stroke the path with.</param>
        public void StrokeColour(Colour colour)
        {
            var state = _stateManager.GetState();
            state.Stroke = new Paint(colour);
        }

        /// <summary>
        /// Sets the current stroke style to a paint, which can be one
        /// of the gradients or patterns.
        /// </summary>
        /// <param name="paint">The stroke to use when stroking the path.</param>
        public void StrokePaint(Paint paint)
        {
            var state = _stateManager.GetState();
            paint.XForm = Maths.TransformMultiply(paint.XForm, state.Transform);
            state.Stroke = paint;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets the current fill style to a solid colour.
        /// </summary>
        /// <param name="colour">The colour to fill the path with.</param>
        public void FillColour(Colour colour)
        {
            var state = _stateManager.GetState();
            state.Fill = new Paint(colour);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets the current fill style to a paint, which can be one
        /// of the gradients or patterns.
        /// </summary>
        /// <param name="paint">The paint to use when filling the path.</param>
        public void FillPaint(Paint paint)
        {
            var state = _stateManager.GetState();
            paint.XForm = Maths.TransformMultiply(paint.XForm, state.Transform);
            state.Fill = paint;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets the miter limit of the stroke style.
        /// The miter limit controls when a shape corner is beveled.
        /// </summary>
        /// <param name="limit">The new miter limit</param>
        public void MiterLimit(float limit)
        {
            var state = _stateManager.GetState();
            state.MiterLimit = limit;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Stes the stroke width of the stroke style.
        /// </summary>
        /// <param name="size">The new size of the stroke width</param>
        public void StrokeWidth(float size)
        {
            var state = _stateManager.GetState();
            state.StrokeWidth = size;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets how the end of the line is drawn.
        /// Can be one of <see cref="LineCap.Butt"/> (default), <see cref="LineCap.Round"/> and <see cref="LineCap.Square"/>
        /// </summary>
        /// <param name="cap">The new line cap</param>
        public void SetLineCap(LineCap cap)
        {
            var state = _stateManager.GetState();
            state.LineCap = cap;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets how sharp path corners are drawn.
        /// Can be one of <see cref="LineCap.Miter"/> (default), <see cref="LineCap.Round"/> and <see cref="LineCap.Bevel"/>
        /// </summary>
        /// <param name="join"></param>
        public void SetLineJoin(LineCap join)
        {
            var state = _stateManager.GetState();
            state.LineJoin = join;
        }

        /// <summary>
        /// <inheritdoc cref="Docs.RenderStyles"/>
        /// 
        /// Sets the transparency applied to all rendered shapes.
        /// Already transparent paths will get proporionally more transparent as well.
        /// </summary>
        /// <param name="alpha">The new global alpha value</param>
        public void GlobalAlpha(float alpha)
        {
            var state = _stateManager.GetState();
            state.Alpha = alpha;
        }
        #endregion

        #region Transforms

        public Matrix3X2<float> TransformRotate(Matrix3X2<float> t, float angle)
        {
            return Maths.TransformRotate(t, angle);
        }

        #endregion

        #region Images
        /// <summary>
        /// <inheritdoc cref="Docs.Images"/>
        /// 
        /// Creates an image by loading it from the disk from the specified file name.<br/>
        /// </summary>
        /// <param name="fileName">The file location of the image</param>
        /// <param name="imageFlags">The image flags</param>
        /// <returns>A handle to the image.</returns>
        public int CreateImage(string fileName, uint imageFlags)
        {
            return ImageLoader.LoadImage(fileName, imageFlags, _graphicsManager);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Images"/>
        /// 
        /// Creates an image by loading it from the specified byte array.<br/>
        /// </summary>
        /// <param name="fileName">The file location of the image</param>
        /// <param name="imageFlags">The image flags</param>
        /// <returns>A handle to the image.</returns>
        public int CreateImageMem(uint imageFlags, byte[] data)
        {
            return ImageLoader.LoadImageMem(imageFlags, data, _graphicsManager);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Images"/>
        /// 
        /// Creates an image from the specified image data.<br/>
        /// </summary>
        /// <param name="fileName">The file location of the image</param>
        /// <param name="imageFlags">The image flags</param>
        /// <returns>A handle to the image.</returns>
        public int CreateImageRgba(int width, int height, uint imageFlags, byte[] data)
        {
            return ImageLoader.CreateImageRgba(width, height, imageFlags, data, _graphicsManager);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Images"/>
        /// 
        /// Updates the images data to the specified data.<br/>
        /// </summary>
        /// <param name="image">The handle to the image</param>
        /// <param name="data">The new image data.</param>
        public void UpdateImage(int image, byte[] data)
        {
            var size = _graphicsManager.GetTextureSize(image);
            _graphicsManager.UpdateTexture(image, 0, 0, size.X, size.Y, data);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Images"/>
        /// 
        /// Gets the image's size.
        /// </summary>
        /// <param name="image">The handle to the image.</param>
        /// <returns>The dimensions of the image.</returns>
        public Vector2D<int> ImageSize(int image)
        {
            return _graphicsManager.GetTextureSize(image);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Images"/>
        /// 
        /// Deletes the created image.
        /// </summary>
        /// <param name="image">The handle to the image.</param>
        public void DeleteImage(int image)
        {
            _graphicsManager.DeleteTexture(image);
        }

        #endregion

        #region Paints
        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns a linear gradient.
        /// The gradient is transformed by the current transform when it is passed to <see cref="FillPaint(Paint)"/> or <see cref="StrokePaint(Paint)"/>.
        /// </summary>
        /// <param name="startX">The start X coordinate</param>
        /// <param name="startY">The start Y coordinate</param>
        /// <param name="endX">The end X coordinate</param>
        /// <param name="endY">The end Y coordinate</param>
        /// <param name="innerColour">The start colour</param>
        /// <param name="outerColour">The end colour</param>
        /// <returns>A new linear gradient paint.</returns>
        public Paint LinearGradient(float startX, float startY, float endX, float endY, Colour innerColour, Colour outerColour)
        {
            return Paint.LinearGradient(startX, startY, endX, endY, innerColour, outerColour);
        }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns a box gradient. Box gradient is a feathered rounded rectangle. It is usefull for
        /// rendering drop shadows or hilights for boxes.
        /// 
        /// The gradient is transformed by the current transform when it is passed to <see cref="FillPaint(Paint)"/> or <see cref="StrokePaint(Paint)"/>.
        /// </summary>
        /// <param name="x">The top-left X Position of the rectangle</param>
        /// <param name="y">The top-left Y Position of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <param name="radius">The corner radius</param>
        /// <param name="feather">Defines how blurry the border is</param>
        /// <param name="innerColour">The gradient's inner colour</param>
        /// <param name="outerColour">The gradient's outer colour</param>
        /// <returns>A new box gradient paint.</returns>
        public Paint BoxGradient(float x, float y, float width, float height, float radius, float feather, Colour innerColour, Colour outerColour)
        {
            return Paint.BoxGradient(x, y, width, height, radius, feather, innerColour, outerColour);
        }

        /// <summary>
        /// <inheritdoc cref="Paint"/>
        /// 
        /// Creates and returns a radial gradient.
        /// 
        /// The gradient is transformed by the current transform when passed into <see cref="FillPaint(Paint)"/> or <see cref="StrokePaint(Paint)"/>.
        /// </summary>
        /// <param name="x">The centre X Position</param>
        /// <param name="y">The centre Y Position</param>
        /// <param name="innerRadius">The gradient's inner radius</param>
        /// <param name="outerRadius">The gradient's outer radius</param>
        /// <param name="innerColour">The start colour</param>
        /// <param name="outerColour">The end colour</param>
        /// <returns>A new radial gradient paint.</returns>
        public Paint RadialGradient(float x, float y, float innerRadius, float outerRadius, Colour innerColour, Colour outerColour)
        {
            return Paint.RadialGradient(x, y, innerRadius, outerRadius, innerColour, outerColour);
        }

        public Paint ImagePattern(float x, float y, float w, float h, float angle, int image, float alpha)
        {
            return Paint.ImagePattern(x, y, w, h, angle, image, alpha);
        }

        #endregion

        #region Paths
        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Clear the current path and sub-path.
        /// </summary>
        public void BeginPath()
        {
            _instructionManager.Clear();
            _pathCache.Clear();
        }

        /// <summary>
        /// Get the Draw object of this context. That is where all render path methods
        /// are stored and implemented and can be called from. They are also implemented in here, which is the preffered option.
        /// </summary>
        public Draw Draw => _draw;

        /// <summary>
        /// <inheritdoc cref="Draw.MoveTo(float, float)"/>
        /// </summary>
        public void MoveTo(float x, float y)
        {
            _draw.MoveTo(x, y);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.LineTo(float, float)"/>
        /// </summary>
        public void LineTo(float x, float y)
        {
            _draw.LineTo(x, y);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.BezierTo(float, float, float, float, float, float)"/>
        /// </summary>
        public void BezierTo(float cx1, float cy1, float cx2, float cy2, float x, float y)
        {
            _draw.BezierTo(cx1, cy1, cx2, cy2, x, y);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.QuadTo(float, float, float, float)"/>
        /// </summary>
        public void QuadTo(float cx, float cy, float x, float y)
        {
            _draw.QuadTo(cx, cy, x, y);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.ArcTo(float, float, float, float, float)"/>
        /// </summary>
        public void ArcTo(float x1, float y1, float x2, float y2, float radius)
        {
            _draw.ArcTo(x1, y1, x2, y2, radius);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.Arc(float, float, float, float, float, Winding)"/>
        /// </summary>
        public void Arc(float x, float y, float r, float a0, float a1, Winding dir = Winding.CCW)
        {
            _draw.Arc(x, y, r, a0, a1, dir);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Close the current sub-path with a line segment.
        /// </summary>
        public void ClosePath()
        {
            var sequence = new InstructionSequence(1);
            sequence.AddClose();
            _instructionManager.AddSequence(sequence, _stateManager.GetState());
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Sets the current sub path winding.
        /// <see cref="Winding"/> and <seealso cref="Solidity"/>
        /// </summary>
        /// <param name="direction">The new direction to be winding in.</param>
        public void PathWinding(Winding direction)
        {
            var sequence = new InstructionSequence(1);
            sequence.AddWinding(direction);
            _instructionManager.AddSequence(sequence, _stateManager.GetState());
        }

        /// <summary>
        /// <inheritdoc cref="Draw.Rect(float, float, float, float)"/>
        /// </summary>
        public void Rect(float x, float y, float width, float height)
        {
            _draw.Rect(x, y, width, height);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.RoundedRect(float, float, float, float, float)"/>
        /// </summary>
        public void RoundedRect(float x, float y, float width, float height, float radius)
        {
            _draw.RoundedRect(x, y, width, height, radius);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.RoundedRectVarying(float, float, float, float, float, float, float, float)"/>
        /// </summary>
        public void RoundedRectVarying(float x, float y, float width, float height, float radTopLeft, float radTopRight, float radBottomRight, float radBottomLeft)
        {
            _draw.RoundedRectVarying(x, y, width, height, radTopLeft, radTopRight, radBottomRight, radBottomLeft);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.Ellipse(float, float, float, float)"/>
        /// </summary>
        public void Ellipse(float x, float y, float radiusX, float radiusY)
        {
            _draw.Ellipse(x, y, radiusX, radiusY);
        }

        /// <summary>
        /// <inheritdoc cref="Draw.Circle(float, float, float)"/>
        /// </summary>
        public void Circle(float x, float y, float radius)
        {
            _draw.Circle(x, y, radius);
        }

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Fills the current path with the current fill style.
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

        /// <summary>
        /// <inheritdoc cref="Docs.Paths"/>
        /// 
        /// Surrounds the current path with the current stroke style.
        /// </summary>
        public void Stroke()
        {
            var state = _stateManager.GetState();
            float scale = Maths.GetAverageScale(state.Transform);
            float strokeWidth = Maths.Clamp(state.StrokeWidth * scale, 0.0f, 200.0f);
            var strokePaint = state.Stroke;
            var inner = strokePaint.InnerColour;
            var outer = strokePaint.OuterColour;

            if (strokeWidth < _style.FringeWidth)
            {
                float alpha = Maths.Clamp(strokeWidth / _style.FringeWidth, 0.0f, 1.0f);
                inner.A *= alpha * alpha;
                outer.A *= alpha * alpha;
                strokeWidth = _style.FringeWidth;
            }

            inner.A *= state.Alpha;
            outer.A *= state.Alpha;

            _pathCache.FlattenPaths(_instructionManager, _style);

            if (_graphicsManager.LaunchParameters.Antialias && state.ShapeAntiAlias)
            {
                _pathCache.ExpandStroke(strokeWidth * 0.5f, _style.FringeWidth, state.LineCap, state.LineJoin, state.MiterLimit, _style);
            }
            else
            {
                _pathCache.ExpandStroke(strokeWidth * 0.5f, 0.0f, state.LineCap, state.LineJoin, state.MiterLimit, _style);
            }

            _graphicsManager.Stroke(strokePaint, state.CompositeOperation, state.Scissor, _style.FringeWidth, strokeWidth, _pathCache.Paths);

            for (int i = 0; i < _pathCache.Paths.Count; i++)
            {
                var path = _pathCache.Paths[i];
                _frameMeta.StrokeTriCount += path.Stroke.Count - 2;
                _frameMeta.DrawCallCount++;
            }

        }
        #endregion

    }
}