using System;

namespace SilkyNvg.Rendering.Vulkan
{

    [Flags]
    public enum CreateFlags
    {

        Antialias = 1 << 0,
        StencilStrokes = 1 << 1,
        Debug = 1 << 2

    }
}
