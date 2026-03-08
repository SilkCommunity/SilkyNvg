using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct BezierToCommand : IPathCommand
    {

        private readonly Vector2 _cp1;
        private readonly Vector2 _cp2;
        private readonly Vector2 _p;

        internal BezierToCommand(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            _cp1 = cp1;
            _cp2 = cp2;
            _p = p;
        }
        

        private static void SplitAndAddBezier(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, Vector2 p0, Vector2 cp1, Vector2 cp2, Vector2 p, RenderTolerances tolerances)
        {
            float upperBoundCurveLength = (cp1 - p0).Length() + (cp2 - cp1).Length() + (p - cp2).Length();
            if (upperBoundCurveLength > tolerances.MaxPathLength)
            {
                var left = new Vector2[4];
                var right = new Vector2[4];
                var points = new Vector2[4] { p0, cp1, cp2, p };
                Geometry.SplitBezier(0.5f, points, left, right);

                SplitAndAddBezier(vertices, vertexIndices, commands, left[0], left[1], left[2], left[3], tolerances);
                SplitAndAddBezier(vertices, vertexIndices, commands, right[0], right[1], right[2], right[3], tolerances);
            }
            
            commands.Add(CommandType.CubicBezierTo);
            vertexIndices.Add((uint)vertices.Count);
            //vertices.Add(cp1);
            //vertices.Add(cp2);
            vertices.Add(p);
        }
        
        private static void Monotonise(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, Vector2 p0, Vector2 cp1, Vector2 cp2, Vector2 p, RenderTolerances tolerances)
        {
            var left = new Vector2[4];
            var right = new Vector2[4];
            var points = new Vector2[4] { p0, cp1, cp2, p };

            Vector2 a = 3 * (-p0 + 3 * cp1 - 3 * cp2 + p);
            Vector2 b = 6 * (p0 - 2 * cp1 + cp2);
            Vector2 c = 3 * (-p0 + cp1);

            var roots = new SortedSet<float>();
            if (Geometry.QuadraticRoots(a.X, b.X, c.X, tolerances.FloatCompareTol, out float tx1, out float tx2))
            {
                roots.Add(tx1);
                roots.Add(tx2);
            }
            if (Geometry.QuadraticRoots(a.Y, b.Y, c.Y, tolerances.FloatCompareTol, out float ty1, out float ty2))
            {
                roots.Add(ty1);
                roots.Add(ty2);
            }

            foreach (var t in roots.Where(t => (tolerances.FloatCompareTol < t) && (t < 1.0f - tolerances.FloatCompareTol)))
            {
                Geometry.SplitBezier(t, points, left, right);
                
                SplitAndAddBezier(vertices, vertexIndices, commands, left[0], left[1], left[2], left[3], tolerances);
                Monotonise(vertices, vertexIndices, commands, right[0], right[1], right[2], right[3], tolerances);
                return;
            }
            
            SplitAndAddBezier(vertices, vertexIndices, commands, points[0], points[1], points[2], points[3], tolerances);
        }

        public uint Build(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, uint firstPathVertexIndex, RenderTolerances tolerances)
        {
            Vector2 p0 = vertices[vertices.Count - 1];
            int commandCountBefore = commands.Count;

            Monotonise(vertices, vertexIndices, commands, p0, _cp1, _cp2, _p, tolerances);

            return (uint)(commands.Count - commandCountBefore);
        }
        
    }   
}