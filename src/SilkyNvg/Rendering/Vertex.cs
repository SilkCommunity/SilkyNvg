using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex
{

    public const int VertexFlagPolynomial = 0;
    public const int VertexFlagEllipse = 1;

    public readonly Vector2 Position;
    public readonly Vector3 Klm;
    public readonly int Flags;
    
    internal Vertex(Vector2 p, Vector3 klm, int flags)
    {
        Position = p;
        Klm = klm;
        Flags = flags;
    }

}