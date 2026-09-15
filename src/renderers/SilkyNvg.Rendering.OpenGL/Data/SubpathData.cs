using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.OpenGL.Data;

[StructLayout(LayoutKind.Explicit)]
internal struct SubpathData
{

    [FieldOffset(0)]
    internal readonly uint SegmentIndex;
    
    [FieldOffset(4)]
    internal uint SegmentCount;

    [FieldOffset(8)]
    // | empty (3 bytes) | bool closed (1 byte) |
    internal readonly int Closed;

    internal SubpathData(uint segmentIndex, uint segmentCount, bool closed)
    {
        SegmentIndex = segmentIndex;
        SegmentCount = segmentCount;
        Closed = closed ? 1 : 0;
    }
    
}