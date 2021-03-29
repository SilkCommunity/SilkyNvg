namespace SilkyNvg.Common
{
    internal static class Docs
    {

        /// <summary>
        /// Colours in NanoVG are stored as floats in RGBA format.<br/>
        /// <br/>
        /// </summary>
        public static void Colours() { }


        /// <summary>
        /// NanoVG uses render states to represent how paths will be<br/>
        /// rendered. The state contains the transform, fill and stroke styles, text and font<br/>
        /// styles and scissor clippings.<br/>
        /// <br/>
        /// </summary>
        public static void States() { }

        /// <summary>
        /// Fill and stroke render style can be either a solid colour or a paint which is a gradient or a pattern.<br/>
        /// Solid colour is simply defined as a colour value, different kinds of paints can be created<br/>
        /// using <see cref="LinearGradient"/>, <see cref="BoxGradient"/>, <see cref="RadialGradient"/> and <see cref="ImagePattern"/><br/>
        /// <br/>
        /// The current render style can be saved and restored using <see cref="Save"/> and <see cref="Restore"/><br/>
        /// <br/>
        /// </summary>
        public static void RenderStyles() { }

        /// <summary>
        /// Drawing a new shape starts with <see cref="BeginPath"/>; it clears all the currently defined<br/>
        /// paths. Then you define one or more paths and sub-paths... ...which describe the shape. These are<br/>
        /// functions to draw common shapes like rectangles and circles and lower level step-by-step functions which<br/>
        /// allow to define a path curve by curve.<br/>
        /// <br/>
        /// NanoVG uses even-odd fill rule to draw the shapes. Solid shapes should have counter-clockwise<br/>
        /// winding and holes should have counter-clockwise order. To specify winding a path you can call<br/>
        /// <see cref="PathWinding"/>. This is usefull especially for the common shapes, which are drawn CCW.<br/>
        /// <br/>
        /// Finally you can fill the path using the current fill style by calling <see cref="Fill"/>, and stroke<br/>
        /// it with current stroke style by calling <see cref="Stroke"/>.<br/>
        /// <br/>
        /// The curve segments and sub-paths... are transformed by the current transform. <br/>
        /// <br/>
        /// </summary>
        public static void Paths() { }

        /// <summary>
        /// NanoVG supports four types of paints: linear gradient, box gradient, radial gradient and image pattern.<br/>
        /// These can be used as paints for strokes and fills. <br/>
        /// <br/>
        /// </summary>
        public static void Paints() { }

        /// <summary>
        /// The paths, gradients, patterns and scissor region are transformed by a transformation<br/>
        /// matrix at the time when they are parsed to the API.<br/>
        /// The current transformation matrix is an affine matrix:<br/>
        /// [sx kx tx]<br/>
        /// [ky sy ty]<br/>
        /// [00 00 01]<br/>
        /// Where: sx and sy define scaling, kx and ky skewing and tx and ty translation.<br/>
        /// The last row is assumed to be 0, 0, 1 and is not stored.<br/>
        /// <br/>
        /// Apart from ResetTransform(), each transformation function first creates a<br/>
        /// specific transformation matrix and pre-multiplies the current transformation by it.<br/>
        /// <br/>
        /// Current coordinate system (transformation) can be saved and restored using Save() and Restore().<br/>
        /// <br/>
        /// </summary>
        public static void Transforms() { }

        /// <summary>
        /// <inheritdoc cref="Transforms"/>
        /// The following functions can be used to make calculations on 3X2 transformation matrices.<br/>
        /// <br/>
        /// </summary>
        public static void Transforms2() { }

        /// <summary>
        /// NanoVG allows you to load jpg, png, psd, tga, pic and gif files to be used for rendering. <br/>
        /// In adition you can upload your own image. The image loading is provided by StbImageSharp (<see cref="https://github.com/StbSharp/StbImageSharp"/>) <br/>
        /// The parameter imageFlags is a combination of flags defined in SilkyNvg.Image.ImageFlags.<br/>
        /// <br/>
        /// </summary>
        public static void Images() { }

        /// <summary>
        /// The composite operations in NanoVG are modeled after the HTML Canvas API and<br/>
        /// the blend func is based on OpenGL (see corresponding manuals for more info).<br/>
        /// The colours in the blending state have premultiplied alpha.<br/>
        /// <br/>
        /// </summary>
        public static void CompositeOperation() { }

        /// <summary>
        /// Scissoring allows you to clip the rendering into a rectangle. This is usefull for various<br/>
        /// user interface cases like rendering a text edit or a timeline.<br/>
        /// <br/>
        /// </summary>
        public static void Scissoring() { }

    }
}
