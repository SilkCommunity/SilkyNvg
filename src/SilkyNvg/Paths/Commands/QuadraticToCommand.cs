using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Paths.Commands;

internal struct QuadraticToCommand(Vector2 cp, Vector2 p) : IPathCommand
{
    public void Flatten(List<Vector2> vertices)
    {
        vertices.Add(cp);
        vertices.Add(p);
    }
}