namespace SilkyNvg.Common
{
    internal static class Docs
    {

        /// <summary>
        /// Colours in NanoVG are stored as floats in RGBA format.<br/>
        /// </summary>
        public static void Colours() { }


        /// <summary>
        /// NanoVG uses render states to represent how paths will be
        /// rendered. The state contains the transform, fill and stroke styles, text and font
        /// styles and scissor clippings.<br/>
        /// </summary>
        public static void States() { }

        /// <summary>
        /// Fill and stroke render style can be either a solid colour or a paint which is a gradient or a pattern.
        /// Solid colour is simply defined as a colour value, different kinds of paints can be created
        /// using <see cref="LinearGradient"/>, <see cref="BoxGradient"/>, <see cref="RadialGradient"/> and <see cref="ImagePattern"/>
        /// 
        /// The current render style can be saved and restored using <see cref="Save"/> and <see cref="Restore"/><br/>
        /// </summary>
        public static void RenderStyles() { }

        /// <summary>
        /// Drawing a new shape starts with <see cref="BeginPath"/>; it clears all the currently defined
        /// paths. Then you define one or more paths and sub-paths... ...which describe the shape. These are
        /// functions to draw common shapes like rectangles and circles and lower level step-by-step functions which
        /// allow to define a path curve by curve.
        ///
        /// 
        /// NanoVG uses even-odd fill rule to draw the shapes. Solid shapes should have counter-clockwise
        /// winding and holes should have counter-clockwise order. To specify winding a path you can call
        /// <see cref="PathWinding"/>. This is usefull especially for the common shapes, which are drawn CCW.
        /// 
        /// Finally you can fill the path using the current fill style by calling <see cref="Fill"/>, and stroke
        /// it with current stroke style by calling <see cref="Stroke"/>.
        /// 
        /// The curve segments and sub-paths... are transformed by the current transform. <br/>
        /// </summary>
        public static void Paths() { }

        /// <summary>
        /// NanoVG supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.
        /// These can be used as paints for strokes and fills.
        /// </summary>
        public static void Paints() { }

    }
}
