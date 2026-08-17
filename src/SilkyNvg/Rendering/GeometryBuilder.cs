using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering;

internal class GeometryBuilder
{

    private readonly DataAccessibleArrayList<Vertex> _vertices = new();
    private readonly List<PathData> _paths = [];

    internal Vertex[] VertexData => _vertices.Data;

    internal uint VertexCount => (uint)_vertices.Count;

    internal IReadOnlyList<PathData> Paths => _paths;

    // Current path data
    private Vector2 _pathStart;
    private int _currentFirstVertex;
    private Vector4 _currentBounds; // min x | min y | max x | max y
    
    internal void BeginPath(Vector2 start)
    {
        _pathStart = start;
        _currentFirstVertex = _vertices.Count;
        _currentBounds = new Vector4(start.X, start.Y, start.X, start.Y);
    }

    internal void EndPath()
    {
        // Add covering rectangle for rendering paint
        // We do it this way, so that the renderer can easily use the same shader for stenciling and painting
        //  operations. This avoids costly pipeline switches, instead you just render another two triangles
        //  which have a flag that tells the shader to render the paint instead of the stenciling.
        //  Notice that
        //      (a) stencil operation is applied before shading, so we avoid calculating color of pixels we don't need
        //      (b) the stenciling / drawing flag is always either true or false in the same draw call. Therefore
        //          the GPU takes little issue with the if-statement in the shader.
        int coverStart = _vertices.Count;
        _vertices.AddRange(
            new Vertex(new Vector2(_currentBounds.X, _currentBounds.Y), Vector3.Zero, VertexFlags.None),
            new Vertex(new Vector2(_currentBounds.X, _currentBounds.W), Vector3.Zero, VertexFlags.None),
            new Vertex(new Vector2(_currentBounds.Z, _currentBounds.W), Vector3.Zero, VertexFlags.None),
            
            new Vertex(new Vector2(_currentBounds.X, _currentBounds.Y), Vector3.Zero, VertexFlags.None),
            new Vertex(new Vector2(_currentBounds.Z, _currentBounds.W), Vector3.Zero, VertexFlags.None),
            new Vertex(new Vector2(_currentBounds.Z, _currentBounds.Y), Vector3.Zero, VertexFlags.None)
        );
        
        _paths.Add(new PathData(
            FirstVertex: _currentFirstVertex,
            VertexCount: (uint)(coverStart - _currentFirstVertex),
            CoverStart: coverStart
        ));
    }

    private void ExpandBounds(Vector2 p)
    {
        _currentBounds.X = MathF.Min(_currentBounds.X, p.X);
        _currentBounds.Y = MathF.Min(_currentBounds.Y, p.Y);
        _currentBounds.Z = MathF.Max(_currentBounds.Z, p.X);
        _currentBounds.W = MathF.Max(_currentBounds.W, p.Y);
    }

    private void UpdateBounds(Vector4 segmentBounds)
    {
        _currentBounds.X = MathF.Min(_currentBounds.X, segmentBounds.X);
        _currentBounds.Y = MathF.Min(_currentBounds.Y, segmentBounds.Y);
        _currentBounds.Z = MathF.Max(_currentBounds.Z, segmentBounds.Z);
        _currentBounds.W = MathF.Max(_currentBounds.W, segmentBounds.W);
    }
    
    private void AddAnchorGeometry(Vector2 first, Vector2 last)
    {
        // Make sure triangle is a triangle
        if (_pathStart.FpEquals(first)) return;
        if (_pathStart.FpEquals(last)) return;
        if (first.FpEquals(last)) return;
        
        _vertices.AddRange(
            new Vertex(_pathStart, Vector3.One, VertexFlags.Stencil),
            new Vertex(first, Vector3.One, VertexFlags.Stencil),
            new Vertex(last, Vector3.One, VertexFlags.Stencil)
        );
    }
    
    internal void AddLine(Vector2 p0, Vector2 p1)
    {
        AddAnchorGeometry(p0, p1);
        ExpandBounds(p1);
    }
    
    internal void AddQuadratic(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        _vertices.AddRange(
            new Vertex(p0, Vector3.Zero, VertexFlags.Stencil),
            new Vertex(p1, new Vector3(0.5f, 0f, 0.5f), VertexFlags.Stencil),
            new Vertex(p2, Vector3.One, VertexFlags.Stencil)
        );

        AddAnchorGeometry(p0, p2);
        ExpandBounds(p1);
        ExpandBounds(p2);
    }
    
    private void SplitCubicAndAdd(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float splitTime)
    {
        Span<Vector2> points = [p0, p1, p2, p3];
        Span<Vector2> left = stackalloc Vector2[4];
        Span<Vector2> right = stackalloc Vector2[4];

        Bezier.SplitBezier(splitTime, points, left, right);

        AddCubic(left[0], left[1], left[2], left[3]);
        AddCubic(right[0], right[1], right[2], right[3]);
    }
    
    internal void AddCubic(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
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
                SplitCubicAndAdd(p0, p1, p2, p3, vd.X / vd.Y);
                return;
            }

            if (!ve.Y.FpEquals(0) && (ve.X / ve.Y) < (1 - Maths.FloatEpsilon) && (ve.X / ve.Y) > Maths.FloatEpsilon)
            {
                SplitCubicAndAdd(p0, p1, p2, p3, ve.X / ve.Y);
                return;
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
            AddQuadratic(
                p0,
                1.5f * p1 - 0.5f * p0,
                p3
            );
            return;
        }
        else if (d1.FpEquals(0.0f) && d2.FpEquals(0.0f) && d3.FpEquals(0.0f)) // Degenerate line or quadratic
        {
            AddLine(p0, p3);
            return;
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
            return;
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

            _vertices.AddRange(
                new Vertex(points[orderedHull[0]], new Vector3(outside * C[orderedHull[0], 0], outside * C[orderedHull[0], 1], C[orderedHull[0], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[1]], new Vector3(outside * C[orderedHull[1], 0], outside * C[orderedHull[1], 1], C[orderedHull[1], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[2]], new Vector3(outside * C[orderedHull[2], 0], outside * C[orderedHull[2], 1], C[orderedHull[2], 2]), VertexFlags.Stencil)
            );
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
            
            _vertices.AddRange(
                new Vertex(points[orderedHull[0]], new Vector3(outside1 * C[orderedHull[0], 0], outside1 * C[orderedHull[0], 1], C[orderedHull[0], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[1]], new Vector3(outside1 * C[orderedHull[1], 0], outside1 * C[orderedHull[1], 1], C[orderedHull[1], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[2]], new Vector3(outside1 * C[orderedHull[2], 0], outside1 * C[orderedHull[2], 1], C[orderedHull[2], 2]), VertexFlags.Stencil)
            );
            _vertices.AddRange(
                new Vertex(points[orderedHull[0]], new Vector3(outside2 * C[orderedHull[0], 0], outside2 * C[orderedHull[0], 1], C[orderedHull[0], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[2]], new Vector3(outside2 * C[orderedHull[2], 0], outside2 * C[orderedHull[2], 1], C[orderedHull[2], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[3]], new Vector3(outside2 * C[orderedHull[3], 0], outside2 * C[orderedHull[3], 1], C[orderedHull[3], 2]), VertexFlags.Stencil)
            );
        }
        else // p1 and p2 are on the same side of (p0, p3), the outside test is the same for both triangles
        {
            int outside = 1;
            if (C[1, 0] * C[1, 0] * C[1, 0] - C[1, 1] * C[1, 2] < 0)
            {
                outside = -1;
            }
            
            _vertices.AddRange(
                new Vertex(points[orderedHull[0]], new Vector3(outside * C[orderedHull[0], 0], outside * C[orderedHull[0], 1], C[orderedHull[0], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[1]], new Vector3(outside * C[orderedHull[1], 0], outside * C[orderedHull[1], 1], C[orderedHull[1], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[2]], new Vector3(outside * C[orderedHull[2], 0], outside * C[orderedHull[2], 1], C[orderedHull[2], 2]), VertexFlags.Stencil)
            );
            _vertices.AddRange(
                new Vertex(points[orderedHull[0]], new Vector3(outside * C[orderedHull[0], 0], outside * C[orderedHull[0], 1], C[orderedHull[0], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[2]], new Vector3(outside * C[orderedHull[2], 0], outside * C[orderedHull[2], 1], C[orderedHull[2], 2]), VertexFlags.Stencil),
                new Vertex(points[orderedHull[3]], new Vector3(outside * C[orderedHull[3], 0], outside * C[orderedHull[3], 1], C[orderedHull[3], 2]), VertexFlags.Stencil)
            );
        }
        
        AddAnchorGeometry(p0, p3);
        ExpandBounds(p1);
        ExpandBounds(p2);
        ExpandBounds(p3);
    }

    internal void AddArcTo(Vector2 start, Vector2 p1, Vector2 end, Vector2 center, float radius)
    {
        // Calculate implicit coordinates
        Vector2 implicitStart = (start - center) / radius;
        Vector2 implicit1 = (p1 - center) / radius;
        Vector2 implicitEnd = (end - center) / radius;
        
        _vertices.AddRange(
            new Vertex(start, new Vector3(implicitStart, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
            new Vertex(p1, new Vector3(implicit1, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
            new Vertex(end, new Vector3(implicitEnd, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
        );
        
        AddAnchorGeometry(start, end);
        UpdateBounds(new Vector4(
            x: center.X - radius,
            y: center.Y - radius,
            z: center.X + radius,
            w: center.Y + radius
        ));
    }

    internal void AddEllipse(Vector2 origin, float radiusX, float radiusY, float rotation, float startAngle, float endAngle, Vector2 s, Vector2 e)
    {
        var transform = Matrix3x2.CreateRotation(rotation);
        transform.Translation = origin;
            
        // Bounding box bounds
        // Order matters here. It needs to line up with the quadrants!
        Span<Vector2> corners =
        [
            new Vector2(radiusX, radiusY), // bottom right
            new Vector2(-radiusX, radiusY), // bottom left
            new Vector2(-radiusX, -radiusY), // top left
            new Vector2(radiusX, -radiusY), // top right
        ];
        Span<Vector2> implicitCorners = stackalloc Vector2[4];
        
        // Generate implicit corners and transform corners
        for (int i = 0; i < 4; i++)
        {
            Vector2 corner = corners[i];
            implicitCorners[i] = new Vector2(corner.X / radiusX, corner.Y / radiusY);
            corners[i] = Vector2.Transform(corner, transform);
        }
        
        // Update the bounds. This is fortunately exactly the transformed corners.
        UpdateBounds(new Vector4(
            x: MathF.Min(MathF.Min(corners[0].X, corners[1].X), MathF.Min(corners[2].X, corners[3].X)),
            y: MathF.Min(MathF.Min(corners[0].Y, corners[1].Y), MathF.Min(corners[2].Y, corners[3].Y)),
            z: MathF.Max(MathF.Max(corners[0].X, corners[1].X), MathF.Max(corners[2].X, corners[3].X)), 
            w: MathF.Max(MathF.Max(corners[0].Y, corners[1].Y), MathF.Max(corners[2].Y, corners[3].Y))    
        ));
        
        // (cos(alpha), sin(alpha)) circle positions so we don't multiply and divide by radius
        var circleS = new Vector2(MathF.Cos(startAngle), MathF.Sin(startAngle));
        var circleE = new Vector2(MathF.Cos(endAngle), MathF.Sin(endAngle));
            
        // modulo reduced angle
        // negative angles, because angle convention is stupid here.
        float alphaS = Maths.NormaliseAngle(startAngle);
        float alphaE = Maths.NormaliseAngle(endAngle);

        if (endAngle - startAngle >= MathF.Tau) // entire ellipse
        {
            _vertices.AddRange(
                new Vertex(corners[2], new Vector3(implicitCorners[2], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                new Vertex(corners[1], new Vector3(implicitCorners[1], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                new Vertex(corners[0], new Vector3(implicitCorners[0], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                
                new Vertex(corners[2], new Vector3(implicitCorners[2], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                new Vertex(corners[0], new Vector3(implicitCorners[0], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                new Vertex(corners[3], new Vector3(implicitCorners[3], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
            );
            
            AddAnchorGeometry(s, e);
            return;
        }

        int sQuadrant = Maths.Quadrant(alphaS);
        int eQuadrant = Maths.Quadrant(alphaE);
        
        var sPlanar = new Vector2(radiusX * MathF.Cos(alphaS), radiusY * MathF.Sin(alphaS));
        var ePlanar = new Vector2(radiusX * MathF.Cos(alphaE), radiusY * MathF.Sin(alphaE));

        int numberOfQuadrants = (eQuadrant - sQuadrant) % 4;
        numberOfQuadrants = numberOfQuadrants >= 0 ? numberOfQuadrants : numberOfQuadrants + 4;
        
        /*
         * It might not be the prettiest, that we have hardcoded the triangulations for all four polygon cases.
         * However, this avoids any triangulation algorithms. So it's probably simpler and faster.
         */
        Vector2 c0, c1, c2, c3;
        int m1Quadrant, m2Quadrant, m3Quadrant;
        switch (numberOfQuadrants)
        {
            case 0:
                // S and E Quadrants are equal here

                // We need two cases here. Either we have to remain in the quadrant
                // or go around once.
                if (alphaS > alphaE)  // go around
                {
                    m1Quadrant = (sQuadrant + 1) % 4;
                    m2Quadrant = (sQuadrant + 2) % 4;
                    m3Quadrant = (sQuadrant + 3) % 4;
                    
                    _vertices.AddRange(
                        new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[sQuadrant], new Vector3(implicitCorners[sQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                        new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m2Quadrant], new Vector3(implicitCorners[m2Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                        new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m2Quadrant], new Vector3(implicitCorners[m2Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                        new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m2Quadrant], new Vector3(implicitCorners[m2Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m3Quadrant], new Vector3(implicitCorners[m3Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                        new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[m3Quadrant], new Vector3(implicitCorners[m3Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[eQuadrant], new Vector3(implicitCorners[eQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
                    );
                }
                else // stay in quadrant
                {
                    _vertices.AddRange(
                        new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(corners[sQuadrant], new Vector3(implicitCorners[sQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                        new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
                    ); 
                }
                break;
            case 1:
                _vertices.AddRange(
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[sQuadrant], new Vector3(implicitCorners[sQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[eQuadrant], new Vector3(implicitCorners[eQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                    new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[eQuadrant], new Vector3(implicitCorners[eQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
                );
                break;
            case 2:
                m1Quadrant = (sQuadrant + 1) % 4;
                _vertices.AddRange(
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[sQuadrant], new Vector3(implicitCorners[sQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                    new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[eQuadrant], new Vector3(implicitCorners[eQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
                );
                break;
            case 3:
                // sQuadrant and eQuadrant are equal here
                m1Quadrant = (sQuadrant + 1) % 4;
                m2Quadrant = (sQuadrant + 2) % 4;
                m3Quadrant = (sQuadrant + 3) % 4;
                _vertices.AddRange(
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[sQuadrant], new Vector3(implicitCorners[sQuadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry),
                    new Vertex(corners[m1Quadrant], new Vector3(implicitCorners[m1Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m2Quadrant], new Vector3(implicitCorners[m2Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                    new Vertex(s, new Vector3(circleS, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m2Quadrant], new Vector3(implicitCorners[m2Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    
                    new Vertex(e, new Vector3(circleE, 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m2Quadrant], new Vector3(implicitCorners[m2Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil),
                    new Vertex(corners[m3Quadrant], new Vector3(implicitCorners[m3Quadrant], 0), VertexFlags.EllipseGeometry | VertexFlags.Stencil)
                );
                break;
            default:
                // there are only four quadrants by definition of the word quadrant.
                return;
        }
        
        AddAnchorGeometry(s, e);
    }

    internal void ClearGeometry()
    {
        _vertices.Clear();
        _paths.Clear();
    }

}