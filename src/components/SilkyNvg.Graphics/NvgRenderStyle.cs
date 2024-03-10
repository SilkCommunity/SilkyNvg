namespace SilkyNvg.Graphics
{
    /// <summary>
    /// <para>Fill and stroke render style can be either a solid colour or a paint which is a gradient or a pattern.
    /// Solid colour is simply defined as a colour value, different kinds of paint can be created using <see cref="Paint.LinearGradient(Silk.NET.Maths.Vector2D{float}, Silk.NET.Maths.Vector2D{float}, Colour, Colour)"/>,
    /// <see cref="Paint.BoxGradient(Silk.NET.Maths.Rectangle{float}, float, float, Colour, Colour)"/>, <see cref="Paint.RadialGradient(Silk.NET.Maths.Vector2D{float}, float, float, Colour, Colour)"/> and
    /// <see cref="Paint.ImagePattern(Silk.NET.Maths.Rectangle{float}, float, int, float)"/></para>
    /// </summary>
    public static class NvgRenderStyle
    {

        /// <summary>
        /// Sets whether to draw antialias for NvgPaths.Stroke() and NvgPaths.Fill(). It's enabled by default.
        /// </summary>
        public static void ShapeAntiAlias(this Nvg nvg, bool enabled)
        {
            nvg.stateStack.CurrentState.ShapeAntiAlias = enabled;
        }

        /// <summary>
        /// Sets the stroke width of the stroke style.
        /// </summary>
        public static void StrokeWidth(this Nvg nvg, float width)
        {
            nvg.stateStack.CurrentState.StrokeWidth = width;
        }

        /// <summary>
        /// Miter limit controls when a sharp corner is beveled.
        /// </summary>
        public static void MiterLimit(this Nvg nvg, float limit)
        {
            nvg.stateStack.CurrentState.MiterLimit = limit;
        }

        /// <summary>
        /// Sets how the end of the line (cap) is drawn.
        /// </summary>
        public static void LineCap(this Nvg nvg, LineCap cap)
        {
            nvg.stateStack.CurrentState.LineCap = cap;
        }
        
        /// <summary>
        /// Sets how the line is drawn.
        /// </summary>
        public static void LineStyle(this Nvg nvg, LineStyle style)
        {
            nvg.stateStack.CurrentState.LineStyle = style;
        }

        /// <summary>
        /// Sets how sharp path corners are drawn.
        /// </summary>
        public static void LineJoin(this Nvg nvg, LineCap join)
        {
            nvg.stateStack.CurrentState.LineJoin = join;
        }

        /// <summary>
        /// Sets the transparency applied to all rendered shapes.
        /// Already transparent paths will get proportionally more transparent as well.
        /// </summary>
        public static void GlobalAlpha(this Nvg nvg, float alpha)
        {
            nvg.stateStack.CurrentState.Alpha = alpha;
        }

        /// <summary>
        /// Sets the current stroke style to a solid colour.
        /// </summary>
        public static void StrokeColour(this Nvg nvg, Colour colour)
        {
            nvg.stateStack.CurrentState.Stroke = new Paint(colour);
        }

        /// <summary>
        /// Sets the current stroke style to a paint, which can be one of the gradients or a pattern.
        /// </summary>
        public static void StrokePaint(this Nvg nvg, Paint paint)
        {
            paint.MultiplyTransform(nvg.stateStack.CurrentState.Transform);
            nvg.stateStack.CurrentState.Stroke = paint;
        }

        /// <summary>
        /// Sets the current fill style to a solid colour.
        /// </summary>
        public static void FillColour(this Nvg nvg, Colour colour)
        {
            nvg.stateStack.CurrentState.Fill = new Paint(colour);
        }

        /// <summary>
        /// Sets the current fill style to a paint, which can be one of the gradients or a pattern.
        /// </summary>
        public static void FillPaint(this Nvg nvg, Paint paint)
        {
            paint.MultiplyTransform(nvg.stateStack.CurrentState.Transform);
            nvg.stateStack.CurrentState.Fill = paint;
        }

    }
}
