using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct LineToCommand : IPathCommand
    {
        
        private readonly Vector2 _p;

        public LineToCommand(Vector2 p)
        {
            _p = p;
        }

        public uint Build(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, uint startVertexIndex, RenderTolerances tolerances)
        {
            Vector2 p0 = vertices[vertices.Count - 1];
            int startCommandCount = commands.Count;
            
            int numberOfSegmentsMinusOne = (int)((_p - p0).Length() / tolerances.MaxPathLength);
            float delta = 1 / (float)numberOfSegmentsMinusOne;
            for (int i = 0; i <= numberOfSegmentsMinusOne; i++)
            {
                float t = i * delta;
                vertices.Add(t * _p + (1 - t) * p0);
                vertexIndices.Add((uint)vertices.Count);
                commands.Add(CommandType.LineTo);
            }
            
            return (uint)(commands.Count - startCommandCount);
        }
        
    }
}
