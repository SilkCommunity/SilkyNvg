using System;

namespace SilkyNvg.Rendering;

[Flags]
public enum VertexFlags : int
{
 
    None = 0,
    
    EllipseGeometry = 1 << 0,
    Stencil = 1 << 1
    
}