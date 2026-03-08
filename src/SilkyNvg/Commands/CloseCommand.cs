using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal struct CloseCommand : IPathCommand
    {
        
        public uint Build(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, uint firstPathVertexIndex, RenderTolerances tolerances)
        {
            Vector2 lastVertex = vertices[vertices.Count - 1];
            Vector2 firstVertex = vertices[(int)firstPathVertexIndex];
            if (lastVertex.FpEquals(firstVertex, tolerances.FloatCompareTol))
            {
                return 0;
            }
            vertexIndices.Add(firstPathVertexIndex);
            commands.Add(CommandType.LineTo);
            return 1;
        }
        
    }
}
