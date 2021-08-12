using System;

namespace SilkyNvg.Images
{

    [Flags]
    public enum ImageFlags
    {

        /// <summary>
        /// Generate mipmaps during creation of the image.
        /// </summary>
        GenerateMimpas = 1 << 0,
        /// <summary>
        /// Repeate image in X direction.
        /// </summary>
        RepeatX = 1 << 1,
        /// <summary>
        /// Repeate image in Y direction.
        /// </summary>
        RepeatY = 1 << 2,
        /// <summary>
        /// Flips (inverses) image in Y direction when rendered.
        /// </summary>
        FlipY = 1 << 3,
        /// <summary>
        /// Image data has premultiplied alpha.
        /// </summary>
        Premultiplied = 1 << 4,
        /// <summary>
        /// Image interpolation is Nearest instead Linear
        /// </summary>
        Nearest = 1 << 5

    }
}
