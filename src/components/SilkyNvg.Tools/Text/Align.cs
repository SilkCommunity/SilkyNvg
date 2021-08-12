using System;

namespace SilkyNvg.Text
{

    [Flags]
    public enum Align
    {

        /// <summary>
        /// Default, align text horizontally to left.
        /// </summary>
        Left = 1 << 0,
        /// <summary>
        /// Align text horizontally to center.
        /// </summary>
        Centre = 1 << 1,
        /// <summary>
        /// Align text horizontally to right.
        /// </summary>
        Right = 1 << 2,

        /// <summary>
        /// Align text vertically to top.
        /// </summary>
        Top = 1 << 3,
        /// <summary>
        /// Align text vertically to middle.
        /// </summary>
        Middle = 1 << 4,
        /// <summary>
        /// Align text vertically to bottom.
        /// </summary>
        Bottom = 1 << 5,
        /// <summary>
        /// Default, align text vertically to baseline.
        /// </summary>
        Baseline = 1 << 6

    }
}
