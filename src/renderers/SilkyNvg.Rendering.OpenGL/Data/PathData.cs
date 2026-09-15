using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.OpenGL.Data;

[StructLayout(LayoutKind.Explicit)]
internal struct PathData
{

    [FieldOffset(0)]
    internal readonly uint SubpathIndex;

    [FieldOffset(4)]
    internal uint SubpathCount;

    internal PathData(uint subpathIndex, uint subpathCount)
    {
        SubpathIndex = subpathIndex;
        SubpathCount = subpathCount;
    }
    
}