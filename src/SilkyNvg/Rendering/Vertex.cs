using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex
{

    public const uint VertexFlagPolynomial = 0;
    public const uint VertexFlagEllipse = 1;

    public readonly Vector2 Position;
    public readonly Vector3 Klm;
    
    internal Vertex(Vector2 p, Vector3 klm)
    {
        Position = p;
        Klm = klm;
    }

}