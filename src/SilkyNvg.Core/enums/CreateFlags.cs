namespace SilkyNvg
{

    public enum CreateFlags
    {

        /// <summary>
        /// Indicates if geometry based AA us used.
        /// </summary>
        Antialias = 1 << 0,
        /// <summary>
        /// Indicates if strokes should be drawing using the stencil buffer.
        /// This slows down drawing, but prevents overlapped double drawing.
        /// </summary>
        StencilStrokes = 1 << 1,
        /// <summary>
        /// Indicates if additional debug checks are done.
        /// </summary>
        Debug = 1 << 2

    }
}
