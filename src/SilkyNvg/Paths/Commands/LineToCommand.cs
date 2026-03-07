using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Paths.Commands;

internal readonly struct LineToCommand(Vector2 p) : IPathCommand
{
    public void Flatten(List<Vector2> vertices)
    {
        vertices.Add(p);
    }
}