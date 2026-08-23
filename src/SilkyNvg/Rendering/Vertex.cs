using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Vertex
{

    public const int StencilMask = 0b100;
    public const int GeometryTypeMask = 0b011;

    internal static Vertex CreateLinear(Vector2 p, Vector3 klm, bool stencil = true)
    {
        return new Vertex(p, klm, stencil, GeometryType.Line);
    }
    
    internal static Vertex CreateRationalQuadratic(Vector2 p, Vector3 klm, bool stencil = true)
    {
        return new Vertex(p, klm, stencil, GeometryType.RationalQuadratic);
    }
    
    internal static Vertex CreateCubic(Vector2 p, Vector3 klm, bool stencil = true)
    {
        return new Vertex(p, klm, stencil, GeometryType.Cubic);
    }
    
    public readonly Vector2 Position;
    public readonly Vector3 Klm;
    
    // Structure | ... | stencil (1 bit) | geometry type (2 bit)
    public readonly int Flags;
    
    internal Vertex(Vector2 p, Vector3 klm, bool stencil = false, GeometryType geometryType = GeometryType.None)
    {
        Position = p;
        Klm = klm;

        Flags = 0;
        Flags |= StencilMask & ((stencil ? 1 : 0) << 2);
        Flags |= GeometryTypeMask & (int)geometryType;
    }

}