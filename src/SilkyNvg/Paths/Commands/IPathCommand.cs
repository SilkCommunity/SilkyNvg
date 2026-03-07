using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Paths.Commands;

internal interface IPathCommand
{

    internal void Flatten(List<Vector2> vertices);

}