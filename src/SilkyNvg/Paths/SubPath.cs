using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<PathSegment> _segments = new List<PathSegment>();
        
        internal bool IsClosed { get; private set; }
        
        internal Vector2 Start => _points[0]; // _points always has at least one element
        
        private Vector2 Last => _points[_points.Count - 1];

        internal SubPath(Vector2 start)
        {
            _points.Add(start);
        }

        private uint AddAnchorGeometry(Vector2 first, Vector2 last, DataAccessibleArrayList<float> verts)
        {
            // Make sure triangle is a triangle
            if (Start.FpEquals(first)) return 0;
            verts.AddRange(
                Start.X, Start.Y, 1f, 1f, 1f,
                first.X, first.Y, 1f, 1f, 1f,
                last.X, last.Y, 1f, 1f, 1f
            );
            return 3;
        }

        private uint AddLineVerts(Vector2 p0, Vector2 p1, DataAccessibleArrayList<float> verts)
        {
            return AddAnchorGeometry(p0, p1, verts);
        }

        private uint AddQuadraticVerts(Vector2 p0, Vector2 p1, Vector2 p2, DataAccessibleArrayList<float> verts)
        {
            verts.AddRange(
                p0.X, p0.Y, 0f, 0f, 0f,
                p1.X, p1.Y, 0.5f, 0f, 0.5f,
                p2.X, p2.Y, 1f, 1f, 1f
            );

            return AddAnchorGeometry(p0, p2, verts) + 3;
        }

        private uint AddCubicVerts(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, DataAccessibleArrayList<float> verts)
        {
            Vector2 q0 = p0;
            Vector2 q1 = -3 * p0 + 3 * p1;
            Vector2 q2 = 3 * p0 - 6 * p1 + 3 * p2;
            Vector2 q3 = -p0 + 3 * p1 - 3 * p2 + p3;

            float d1 = -(q3.X * q2.Y - q3.Y * q2.X);
            float d2 = q3.X * q1.Y - q3.Y * q1.X;
            float d3 = -(q2.X * q1.Y - q2.Y * q1.X);
            
            float discriminant = 3 * d2 * d2 - 4 * d1 * d3;

            float s = 1.0f;
            Matrix4x4 C = Matrix4x4.Identity;
            
            Matrix4x4 F;
            Vector2 vl, vm, vn, vd, ve;
            if (!d1.FpEquals(0.0f) && (discriminant > Maths.FloatEpsilon || discriminant.FpEquals(0.0f))) // Serpentine or Cusp with inflection at infinity
            {
                vl = new Vector2(
                    d2 + (float)Math.Sqrt(discriminant / 3),
                    2 * d1
                );
                vm = new Vector2(
                    d2 - (float)Math.Sqrt(discriminant / 3),
                    2 * d1
                );
                vn = Vector2.UnitX;
                
                // Normalise vl and vm for numerical stability
                vl = Vector2.Normalize(vl);
                vm = Vector2.Normalize(vm);

                F = new Matrix4x4(
                     vl.X * vm.X,                    vl.X * vl.X * vl.X,      vm.X * vm.X * vm.X, 1,
                    -vm.Y * vl.X - vl.Y * vm.X, -3 * vl.Y * vl.X * vl.X, -3 * vm.Y * vm.X * vm.X, 0,
                     vl.Y * vm.Y,                3 * vl.Y * vl.Y * vl.X,  3 * vm.Y * vm.Y * vm.X, 0,
                              0,                    -vl.Y * vl.Y * vl.Y,     -vm.Y * vm.Y * vm.Y, 0
                );
                C = Matrix4x4.Multiply(Maths.M3Inverse, F);

                s = Math.Sign(d1);
            }
            else if (!d1.FpEquals(0.0f) && discriminant < -Maths.FloatEpsilon) // Loop
            {
                vd = new Vector2(
                    d2 + (float)Math.Sqrt(-discriminant),
                    2 * d1
                );
                ve = new Vector2(
                    d2 - (float)Math.Sqrt(-discriminant),
                    2 * d1
                );
                
                // Normalise vd and ve for numerical stability
                vd = Vector2.Normalize(vd);
                ve = Vector2.Normalize(ve);

                F = new Matrix4x4(
                     vd.X * ve.X,                vd.X * vd.X * ve.X,                           vd.X * ve.X * ve.X,                          1,
                    -ve.Y * vd.X - vd.Y * ve.X, -ve.Y * vd.X * vd.X - 2 * vd.Y * ve.X * vd.X, -vd.Y * ve.X * ve.X - 2 * ve.Y * vd.X * ve.X, 0,
                     vd.Y * ve.Y,                ve.X * vd.Y * vd.Y + 2 * ve.Y * vd.X * vd.Y,  vd.X * ve.Y * ve.Y + 2 * vd.Y * ve.X * ve.Y, 0,
                               0,               -vd.Y * vd.Y * ve.Y,                          -vd.Y * ve.Y * ve.Y,                          0
                );
                C = Matrix4x4.Multiply(Maths.M3Inverse, F);
            }
            else if (d1.FpEquals(0.0f) && !d2.FpEquals(0.0f)) // Cusp with cusp at infinity
            {
                vl = new Vector2(
                    d3,
                    3 * d2
                );
                vm = Vector2.UnitX;
                vn = Vector2.UnitX;
                
                // Normalize vl for numerical stability
                vl = Vector2.Normalize(vl);

                F = new Matrix4x4(
                     vl.X,      vl.X * vl.X * vl.X, 1, 1,
                    -vl.Y, -3 * vl.Y * vl.X * vl.X, 0, 0,
                        0,  3 * vl.Y * vl.Y * vl.X, 0, 0,
                        0,     -vl.Y * vl.Y * vl.Y, 0, 0
                );
                C = Matrix4x4.Multiply(Maths.M3Inverse, F);
            }
            else if (d1.FpEquals(0.0f) && d2.FpEquals(0.0f) && !d3.FpEquals(0.0f)) // Degenerate Quadratic
            {
                Vector2 cp = (p1 + p2) / 2.0f;
                return AddQuadraticVerts(p0, cp, p3, verts);
            }
            else if (d1.FpEquals(0.0f) && d2.FpEquals(0.0f) && d3.FpEquals(0.0f)) // Degenerate line or quadratic
            {
                return AddLineVerts(p0, p3, verts);
            }

            // The order p0, p1, p2, p3 matters! This is because it needs to match up with the rows of the C-Matrix for indexing.
            Span<Vector2> spoints = [ p0, p1, p2, p3 ];
            Span<int> ppoints = stackalloc int[5];

            
            
            int nHull = Maths.ConvexHull(spoints, ppoints);
            if (nHull == 4)
            {
                verts.AddRange(
                    p0.X, p0.Y, s * C[0, 0], s * C[0, 1], C[0, 2],
                    p1.X, p1.Y, s * C[1, 0], s * C[1, 1], C[1, 2],
                    p3.X, p3.Y, s * C[3, 0], s * C[3, 1], C[3, 2],
                    
                    p0.X, p0.Y, s * C[0, 0], s * C[0, 1], C[0, 2],
                    p3.X, p3.Y, s * C[3, 0], s * C[3, 1], C[3, 2],
                    p2.X, p2.Y, s * C[2, 0], s * C[2, 1], C[2, 2]
                );
            }
            else if (nHull == 3)
            {
                Console.WriteLine("Triangle");
            }
            else
            {
                throw new Exception("Failed to compute convex hull of cubic bezier curve.");
            }

            return AddAnchorGeometry(p0, p3, verts) + 6;
        }

        internal uint BuildGeometry(DataAccessibleArrayList<float> verts, Matrix3x2 transform)
        {
            int pointIndex = 0;
            uint vertexCount = 0;
            foreach (var segmentType in _segments)
            {
                Vector2 p0, p1, p2, p3;
                switch (segmentType)
                {
                    case PathSegment.Line:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);
                        vertexCount += AddLineVerts(p0, p1, verts);
                        pointIndex += 1;
                        break;
                    case PathSegment.Quadratic:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);
                        p2 = Vector2.Transform(_points[pointIndex + 2], transform);
                        vertexCount += AddQuadraticVerts(p0, p1, p2, verts);
                        pointIndex += 2;
                        break;
                    case PathSegment.Cubic:
                        p0 = Vector2.Transform(_points[pointIndex + 0], transform);
                        p1 = Vector2.Transform(_points[pointIndex + 1], transform);
                        p2 = Vector2.Transform(_points[pointIndex + 2], transform);
                        p3 = Vector2.Transform(_points[pointIndex + 3], transform);
                        vertexCount += AddCubicVerts(p0, p1, p2, p3, verts);
                        pointIndex += 3;  
                        break;
                }
            }

            return vertexCount;
        }

        internal void AddLine(Vector2 p)
        {
            _points.Add(p);
            _segments.Add(PathSegment.Line);
        }

        internal void AddQuadratic(Vector2 cp, Vector2 p)
        {
            if (Last.FpEquals(cp))
            {
                AddLine(p);
            }
            else
            {
                _points.Add(cp);
                _points.Add(p);
                _segments.Add(PathSegment.Quadratic);
            }
        }

        internal void AddCubic(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            if (Last.FpEquals(cp1))
            {
                AddQuadratic(cp2, p);
            }
            else if (cp1.FpEquals(cp2))
            {
                AddQuadratic((cp1 + cp2) / 2, p);
            }
            else
            {
                _points.Add(cp1);
                _points.Add(cp2);
                _points.Add(p);
                _segments.Add(PathSegment.Cubic);
            }
        }

        internal void Close()
        {
            IsClosed = true;
            if (_points.Count > 1) // Don't close if we only have one point anyway
            {
                AddLine(_points[0]);
            }
        }

    }
}