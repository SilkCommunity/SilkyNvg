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

        private uint SplitCubicAndAddVerts(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float splitTime, DataAccessibleArrayList<float> verts)
        {
            Span<Vector2> points = [p0, p1, p2, p3];
            Span<Vector2> left = stackalloc Vector2[4];
            Span<Vector2> right = stackalloc Vector2[4];

            Bezier.SplitBezier(splitTime, points, left, right);

            uint nverts = AddCubicVerts(left[0], left[1], left[2], left[3], verts);
            nverts += AddCubicVerts(right[0], right[1], right[2], right[3], verts);

            return nverts;
        }
        
        private uint AddCubicVerts(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, DataAccessibleArrayList<float> verts)
        {
            /* First convert the control points to the power basis, multiplying by M3
             *
             *                                      |  1 -3  3 -1 |
             * | c0 c1 c2 c3 | = | p0 p1 p2 p3 |    |  0  3 -6  3 |
             *                                      |  0  0  3 -3 |
             *                                      |  0  0  0  1 |
             */
            Vector2 q0 = p0;
            Vector2 q1 = -3 * p0 + 3 * p1;
            Vector2 q2 = 3 * p0 - 6 * p1 + 3 * p2;
            Vector2 q3 = -p0 + 3 * p1 - 3 * p2 + p3;

            /* Compute determinants d0, d1, d2, d3, which give the coefficients of the inflection point polynomial:
             *      I(t, s) = d0 * t^3 - 3 * d1 * t^2 * s + 3 * d2 * t * s^2 - d3 * s^3
             *
             * The roots of I are the inflection points of the parametric curve, in homogeneous
             * coordinates (i.e. we can have an inflection point at infinity with s = 0)
             */
            float d1 = -(q3.X * q2.Y - q3.Y * q2.X);
            float d2 = q3.X * q1.Y - q3.Y * q1.X;
            float d3 = -(q2.X * q1.Y - q2.Y * q1.X);
            
            /*
             * Let D = 3 * d2^2 - 4 * d3 * d1
             * Then discr(I) = d1^2 * (3 * d2^2 - 4 * d3 * d1) = d1^2 * D
             */
            float D = 3 * d2 * d2 - 4 * d1 * d3;

            float s = 1.0f;
            Matrix4x4 C = Matrix4x4.Identity;
            
            if (!d1.FpEquals(0.0f) && D > -Maths.FloatEpsilon) // Serpentine or Cusp with inflection at infinity
            {
                var vl = new Vector2(
                    d2 + (float)Math.Sqrt(D / 3),
                    2 * d1
                );
                var vm = new Vector2(
                    d2 - (float)Math.Sqrt(D / 3),
                    2 * d1
                );
                
                // Normalise vl and vm for numerical stability
                vl = Vector2.Normalize(vl);
                vm = Vector2.Normalize(vm);

                var F = new Matrix4x4(
                     vl.X * vm.X,                    vl.X * vl.X * vl.X,      vm.X * vm.X * vm.X, 1,
                    -vm.Y * vl.X - vl.Y * vm.X, -3 * vl.Y * vl.X * vl.X, -3 * vm.Y * vm.X * vm.X, 0,
                     vl.Y * vm.Y,                3 * vl.Y * vl.Y * vl.X,  3 * vm.Y * vm.Y * vm.X, 0,
                              0,                    -vl.Y * vl.Y * vl.Y,     -vm.Y * vm.Y * vm.Y, 0
                );
                C = Matrix4x4.Multiply(Maths.M3Inverse, F);
            }
            else if (!d1.FpEquals(0.0f) && D < -Maths.FloatEpsilon) // Loop
            {
                var vd = new Vector2(
                    d2 + (float)Math.Sqrt(-D),
                    2 * d1
                );
                var ve = new Vector2(
                    d2 - (float)Math.Sqrt(-D),
                    2 * d1
                );
                
                // Normalise vd and ve for numerical stability
                vd = Vector2.Normalize(vd);
                ve = Vector2.Normalize(ve);
                
                /*
                 * If one of the parameters (td / se) or (te / se) iis in the interval [0, 1], then the double point
                 * is inside the control points vertex hull and would cause a shading anomaly. If this is the case,
                 * subdivide the curve at this point.
                 */
                if (!vd.Y.FpEquals(0) && (vd.X / vd.Y) < (1 - Maths.FloatEpsilon) && (vd.X / vd.Y) > Maths.FloatEpsilon)
                {
                    return SplitCubicAndAddVerts(p0, p1, p2, p3, vd.X / vd.Y, verts);
                }

                if (!ve.Y.FpEquals(0) && (ve.X / ve.Y) < (1 - Maths.FloatEpsilon) && (ve.X / ve.Y) > Maths.FloatEpsilon)
                {
                    return SplitCubicAndAddVerts(p0, p1, p2, p3, ve.X / ve.Y, verts);
                }

                var F = new Matrix4x4(
                     vd.X * ve.X,                vd.X * vd.X * ve.X,                           vd.X * ve.X * ve.X,                          1,
                    -ve.Y * vd.X - vd.Y * ve.X, -ve.Y * vd.X * vd.X - 2 * vd.Y * ve.X * vd.X, -vd.Y * ve.X * ve.X - 2 * ve.Y * vd.X * ve.X, 0,
                     vd.Y * ve.Y,                ve.X * vd.Y * vd.Y + 2 * ve.Y * vd.X * vd.Y,  vd.X * ve.Y * ve.Y + 2 * vd.Y * ve.X * ve.Y, 0,
                               0,               -vd.Y * vd.Y * ve.Y,                          -vd.Y * ve.Y * ve.Y,                          0
                );
                C = Matrix4x4.Multiply(Maths.M3Inverse, F);
            }
            else if (d1.FpEquals(0.0f) && !d2.FpEquals(0.0f)) // Cusp with cusp at infinity
            {
                var vl = new Vector2(
                    d3,
                    3 * d2
                );
                var vm = Vector2.UnitX;
                var vn = Vector2.UnitX;
                
                // Normalize vl for numerical stability
                vl = Vector2.Normalize(vl);

                var F = new Matrix4x4(
                     vl.X,      vl.X * vl.X * vl.X, 1, 1,
                    -vl.Y, -3 * vl.Y * vl.X * vl.X, 0, 0,
                        0,  3 * vl.Y * vl.Y * vl.X, 0, 0,
                        0,     -vl.Y * vl.Y * vl.Y, 0, 0
                );
                C = Matrix4x4.Multiply(Maths.M3Inverse, F);
            }
            else if (d1.FpEquals(0.0f) && d2.FpEquals(0.0f) && !d3.FpEquals(0.0f)) // Degenerate Quadratic
            {
                return AddQuadraticVerts(
                    p0,
                    1.5f * p1 - 0.5f * p0,
                    p3,
                    verts
                );
            }
            else if (d1.FpEquals(0.0f) && d2.FpEquals(0.0f) && d3.FpEquals(0.0f)) // Degenerate line or quadratic
            {
                return AddLineVerts(p0, p3, verts);
            }
            else
            {
                throw new Exception("Cubic curve classification failed.");
            }

            // Compute the convex hull using gift wrapping algorithm
            Span<Vector2> points = [p0, p1, p2, p3];
            Span<int> hull = stackalloc int[4];

            int nHull = Bezier.CubicConvexHull(points, hull);
            
            // re-arrange convex hull s.t. p0 comes first
            int start = -1;
            Span<int> orderedHull = stackalloc int[4];
            for (int i = 0; i < nHull; i++)
            {
                if (hull[i] == 0)
                {
                    start = i;
                }

                if (start >= 0)
                {
                    orderedHull[i - start] = hull[i];
                }
            }
            
            for (int i = 0; i < start; i++)
            {
                orderedHull[nHull - start + i] = hull[i];
            }
            
            // Ignore degenerate hulls
            if (nHull <= 2)
            {
                return 0;
            }
            
            /*
             * Inside / Outside test for the two triangles. In the shader, the outside is defined by k^3 - lm > 0.
             * We flip k and l such that the control points p1 and p2 are always outside the covered area
             */
            if (nHull == 3) // convex hull is a triangle
            {
                /*
                 * The convex hull is a triangle. Possible one of the control points p1 or p2 is inside the covered area. We want to compute
                 * this test for the control point which is part of the convex hull, since this will be outside the convered area.
                 * (Note the convex hull is never part of the covered area).
                 * Since there are 3 points in the hull and p0 is the first and p3 belongs to the hull, this means we must select
                 * the point of the convex hull which is neither the first nor p3.
                 */
                int testPoint = orderedHull[1] == 3 ? orderedHull[2] : orderedHull[1];
                int outside = 1;
                if ((C[testPoint, 0] * C[testPoint, 0] * C[testPoint, 0]) - C[testPoint, 1] * C[testPoint, 2] < 0)
                {
                    outside = -1;
                }

                verts.AddRange(
                    points[orderedHull[0]].X, points[orderedHull[0]].Y, outside * C[orderedHull[0], 0], outside * C[orderedHull[0], 1], C[orderedHull[0], 2],
                    points[orderedHull[1]].X, points[orderedHull[1]].Y, outside * C[orderedHull[1], 0], outside * C[orderedHull[1], 1], C[orderedHull[1], 2],
                    points[orderedHull[2]].X, points[orderedHull[2]].Y, outside * C[orderedHull[2], 0], outside * C[orderedHull[2], 1], C[orderedHull[2], 2]
                );

                return AddAnchorGeometry(p0, p3, verts) + 3;
            }
            // From here on it follows hullCount = 4
            else if (orderedHull[2] == 3) // p1 and p2 are not on the same side of (p0, p3). The outside can be different for the two triangles
            {
                int outside1 = 1;
                int outside2 = 1;

                int test = orderedHull[1];
                if (C[test, 0] * C[test, 0] * C[test, 0] - C[test, 1] * C[test, 2] < 0)
                {
                    outside1 = -1;
                }

                test = orderedHull[3];
                if (C[test, 0] * C[test, 0] * C[test, 0] - C[test, 1] * C[test, 2] < 0)
                {
                    outside2 = -1;
                }
                
                verts.AddRange(
                    points[orderedHull[0]].X, points[orderedHull[0]].Y, outside1 * C[orderedHull[0], 0], outside1 * C[orderedHull[0], 1], C[orderedHull[0], 2],
                    points[orderedHull[1]].X, points[orderedHull[1]].Y, outside1 * C[orderedHull[1], 0], outside1 * C[orderedHull[1], 1], C[orderedHull[1], 2],
                    points[orderedHull[2]].X, points[orderedHull[2]].Y, outside1 * C[orderedHull[2], 0], outside1 * C[orderedHull[2], 1], C[orderedHull[2], 2]
                );
                verts.AddRange(
                    points[orderedHull[0]].X, points[orderedHull[0]].Y, outside2 * C[orderedHull[0], 0], outside2 * C[orderedHull[0], 1], C[orderedHull[0], 2],
                    points[orderedHull[2]].X, points[orderedHull[2]].Y, outside2 * C[orderedHull[2], 0], outside2 * C[orderedHull[2], 1], C[orderedHull[2], 2],
                    points[orderedHull[3]].X, points[orderedHull[3]].Y, outside2 * C[orderedHull[3], 0], outside2 * C[orderedHull[3], 1], C[orderedHull[3], 2]
                );

                return AddAnchorGeometry(p0, p3, verts) + 6;
            }
            else // p1 and p2 are on the same side of (p0, p3), the outside test is the same for both triangles
            {
                int outside = 1;
                if (C[1, 0] * C[1, 0] * C[1, 0] - C[1, 1] * C[1, 2] < 0)
                {
                    outside = -1;
                }
                
                verts.AddRange(
                    points[orderedHull[0]].X, points[orderedHull[0]].Y, outside * C[orderedHull[0], 0], outside * C[orderedHull[0], 1], C[orderedHull[0], 2],
                    points[orderedHull[1]].X, points[orderedHull[1]].Y, outside * C[orderedHull[1], 0], outside * C[orderedHull[1], 1], C[orderedHull[1], 2],
                    points[orderedHull[2]].X, points[orderedHull[2]].Y, outside * C[orderedHull[2], 0], outside * C[orderedHull[2], 1], C[orderedHull[2], 2]
                );
                verts.AddRange(
                    points[orderedHull[0]].X, points[orderedHull[0]].Y, outside * C[orderedHull[0], 0], outside * C[orderedHull[0], 1], C[orderedHull[0], 2],
                    points[orderedHull[2]].X, points[orderedHull[2]].Y, outside * C[orderedHull[2], 0], outside * C[orderedHull[2], 1], C[orderedHull[2], 2],
                    points[orderedHull[3]].X, points[orderedHull[3]].Y, outside * C[orderedHull[3], 0], outside * C[orderedHull[3], 1], C[orderedHull[3], 2]
                );

                return AddAnchorGeometry(p0, p3, verts) + 6;
            }
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