using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Paths.Commands;

internal struct BezierToCommand(Vector2 cp1, Vector2 cp2, Vector2 p) : IPathCommand
{
    public void Flatten(List<Vector2> vertices)
    {
        vertices.Add(cp1);
        vertices.Add(cp2);
        vertices.Add(p);
    }
}