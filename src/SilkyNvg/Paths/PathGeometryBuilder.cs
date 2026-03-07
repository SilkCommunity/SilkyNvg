using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.States;

namespace SilkyNvg.Paths;

internal class PathGeometryBuilder
{

    private readonly StateStack _stateStack;

    internal PathGeometryBuilder(StateStack stateStack)
    {
        _stateStack = stateStack;
    }

    internal void FillPath(Path path, List<Vector2> vertices, List<uint> indices)
    {
        
    }

}