using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct VertUniforms
    {

        [FieldOffset(0)]

        public Vector2D<float> ViewSize;

    }
}
