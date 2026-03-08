using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal interface IPathCommand
    {

        uint Build(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, uint firstPathVertexIndex, RenderTolerances tolerances);

    }
}