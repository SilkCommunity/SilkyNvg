using System.Runtime.InteropServices;
using Silk.NET.Maths;

namespace SilkyNvg.Rendering.Vulkan.Shaders
{
    [StructLayout(LayoutKind.Explicit)]
    public struct VertUniforms
    {

        [FieldOffset(0)]

        public Vector2D<float> ViewSize;

    }
}