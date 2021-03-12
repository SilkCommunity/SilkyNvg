namespace SilkyNvg.Base
{

    /// <summary>
    /// Settings that are set on create..
    /// They get input into the uint - if using more then one combine them using bitwise or.
    /// </summary>
    public enum CreateFlag
    {

        /// <summary>
        /// Wheather geometry-based AA is used. Does not disable MSAA.
        /// </summary>
        EdgeAntialias = 1 << 0,

        /// <summary>
        /// Indicates wheather rendering is done with or without
        /// a stencil buffer. Reduces rendering ammount with overlapping
        /// elements.
        /// </summary>
        StencilStrokes = 1 << 1,

        /// <summary>
        /// Log and do additional debug checks.
        /// </summary>
        Debug = 1 << 2

    }
}