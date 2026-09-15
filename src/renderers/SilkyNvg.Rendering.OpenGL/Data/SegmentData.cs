using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.OpenGL.Data;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct SegmentData
{

    [FieldOffset(0)]
    internal readonly uint VertexIndex;

    [FieldOffset(4)]
    // | empty (1 byte) | empty (1 byte) | bool reversed (1 byte) | byte type (1 byte) |
    internal readonly int ReversedType;

    [FieldOffset(8)]
    internal readonly float ArcW1;

    [FieldOffset(12)]
    internal readonly float Offset;

    internal SegmentData(uint vertexIndex, SegmentType type, bool reversed, float arcW1, float offset)
    {
        VertexIndex = vertexIndex;
        ArcW1 = arcW1;
        Offset = offset;

        ReversedType =
            ((reversed ? 1 : 0) << 8) & 0x0000FF00 |
            ((byte)type << 0) & 0x000000FF;
    }

}