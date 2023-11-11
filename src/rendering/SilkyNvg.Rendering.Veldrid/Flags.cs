using System;

namespace SilkyNvg.Rendering.Vulkan
{

    [Flags]
    public enum RenderFlags : byte
    {

        Antialias = 1 << 0,
        StencilStrokes = 1 << 1,
        Debug = 1 << 2,

    }
}