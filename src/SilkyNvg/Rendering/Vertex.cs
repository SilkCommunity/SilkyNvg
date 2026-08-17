using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex
{
    
    public readonly Vector2 Position;
    public readonly Vector3 Klm;
    public readonly VertexFlags Flags;
    
    internal Vertex(Vector2 p, Vector3 klm, VertexFlags flags)
    {
        Position = p;
        Klm = klm;
        Flags = flags;
    }

}