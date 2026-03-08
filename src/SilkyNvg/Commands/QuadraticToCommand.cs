using System;
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

        private static void Monotonise(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, Vector2 p0, Vector2 cp, Vector2 p, RenderTolerances tolerances)
        {
            var left = new Vector2[3];
            var right = new Vector2[3];
            var points = new Vector2[3] { p0, cp, p };
            
            Vector2 a = p0 - 2 * cp + p;
            Vector2 b = 2 * (-p0 + cp);

            float tx = -b.X / (2.0f * a.X);
            float ty = -b.Y / (2.0f * a.Y);

            float t1 = Math.Min(tx, ty);
            float t2 = Math.Max(tx, ty);

            if ((tolerances.FloatCompareTol < t1) && (t1 < 1.0f - tolerances.FloatCompareTol))
            {
                Geometry.SplitBezier(t1, points, left, right);
                
                SplitAndAddBezier(vertices, vertexIndices, commands, left[0], left[1], left[2], tolerances);
                Monotonise(vertices, vertexIndices, commands, right[0], right[1], right[2], tolerances);
            }
            else if ((tolerances.FloatCompareTol < t2) && (t2 < 1.0f - tolerances.FloatCompareTol))
            {
                Geometry.SplitBezier(t2, points, left, right);
                
                SplitAndAddBezier(vertices, vertexIndices, commands, left[0], left[1], left[2], tolerances);
                Monotonise(vertices, vertexIndices, commands, right[0], right[1], right[2], tolerances);
            }
            else
            {
                SplitAndAddBezier(vertices, vertexIndices, commands, points[0], points[1], points[2], tolerances);
            }
        }

        public uint Build(List<Vector2> vertices, List<uint> vertexIndices,  List<CommandType> commands, uint firstPathVertexIndex, RenderTolerances tolerances)
        {
            Vector2 p0 = vertices[vertices.Count - 1];
            int commandCountBefore = commands.Count;

            Monotonise(vertices, vertexIndices, commands, p0, _cp, _p, tolerances);
            
            return (uint)(commands.Count - commandCountBefore);
        }
    
    }
}