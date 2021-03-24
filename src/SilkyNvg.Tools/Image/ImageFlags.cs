namespace SilkyNvg.Image
{
    /// <summary>
    /// Flags set when creating an image.
    /// </summary>
    public enum ImageFlags
    {

        /// <summary>
        /// Generate mimpas during creation of the image.
        /// </summary>
        GenerateMipmaps = 1 << 0,
        /// <summary>
        /// Repeat image in the X direction
        /// </summary>
        RepeatX = 1 << 1,
        /// <summary>
        /// Repeat image in the Y direction
        /// </summary>
        RepeatY = 1 << 2,
        /// <summary>
        /// Flips (inverses) image in the Y direction when rendered
        /// </summary>
        FlipY = 1 << 3,
        /// <summary>
        /// Image data has premultiplied alpha
        /// </summary>
        Premultiplied = 1 << 4,
        /// <summary>
        /// Image interpolation is Nearest instead of Linear
        /// </summary>
        Nearest = 1 << 5

    }
}
