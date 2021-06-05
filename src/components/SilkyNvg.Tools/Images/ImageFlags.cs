using System;

namespace SilkyNvg.Images
{

    [Flags]
    public enum ImageFlags
    {

        GenerateMimpas = 1 << 0,
        RepeatX = 1 << 1,
        RepeatY = 1 << 2,
        FlipY = 1 << 3,
        Premultiplied = 1 << 4,
        Nearest = 1 << 5

    }
}
