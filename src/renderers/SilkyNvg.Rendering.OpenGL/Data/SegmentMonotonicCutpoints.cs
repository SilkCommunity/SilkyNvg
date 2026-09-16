using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering.OpenGL.Data;

[StructLayout(LayoutKind.Explicit)]
internal readonly struct SegmentMonotonicCutpoints
{

    [FieldOffset(0)]
    internal readonly float Cut0;
    
    [FieldOffset(4)]
    internal readonly float Cut1;
    
    [FieldOffset(8)]
    internal readonly float Cut2;
    
    [FieldOffset(12)]
    internal readonly float Cut3;

    [FieldOffset(16)]
    internal readonly int NCuts;

}