using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct QuadraticToCommand : IPathCommand
    {
    
        private readonly Vector2 _cp;
        private readonly Vector2 _p;

        public QuadraticToCommand(Vector2 cp, Vector2 p)
        {
            _cp = cp;
            _p = p;
        }

        private static void SplitAndAddBezier(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, Vector2 p0, Vector2 cp, Vector2 p, RenderTolerances tolerances)
        {
            float upperBoundCurveLength = (cp - p0).Length() + (p - cp).Length();
            if (upperBoundCurveLength > tolerances.MaxPathLength)
            {
                var left = new Vector2[3];
                var right = new Vector2[3];
                var points = new Vector2[3] { p0, cp, p };
                Geometry.SplitBezier(0.5f, points, left, right);

                SplitAndAddBezier(vertices, vertexIndices, commands, left[0], left[1], left[2], tolerances);
                SplitAndAddBezier(vertices, vertexIndices, commands, right[0], right[1], right[2], tolerances);
            }
            
            commands.Add(CommandType.QuadraticBezierTo);
            vertexIndices.Add((uint)vertices.Count);
            //vertices.Add(cp);
            vertices.Add(p);
        }

        public uint Build(List<Vector2> vertices, List<uint> vertexIndices,  List<CommandType> commands, uint firstPathVertexIndex, RenderTolerances tolerances)
        {
            Vector2 p0 = vertices[vertices.Count - 1];
            int commandCountBefore = commands.Count;

            SplitAndAddBezier(vertices, vertexIndices, commands, p0, _cp, _p, tolerances);

            return (uint)(commands.Count - commandCountBefore);
        }
    
    }
}