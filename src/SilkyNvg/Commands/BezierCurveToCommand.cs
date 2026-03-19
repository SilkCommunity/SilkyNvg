using System;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct BezierCurveToCommand : ICommand
    {

        private readonly Vector2 _cp1;
        private readonly Vector2 _cp2;
        private readonly Vector2 _p;

        internal BezierCurveToCommand(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            _cp1 = cp1;
            _cp2 = cp2;
            _p = p;
        }

        public Vector2 FillToScene(Matrix3x2 transform, Vector2 p0, Vector2 subpathFirstPoint, ref Vector4 bounds, Scene scene, RenderTolerances tol)
        {
            // Get transformed points
            Vector2 cp1 = Vector2.Transform(_cp1, transform);
            Vector2 cp2 = Vector2.Transform(_cp2, transform);
            Vector2 p = Vector2.Transform(_p, transform);
            
            // Bounds
            Vector2 a = -p0 + 3 * cp1 - 3 * cp2 + p;
            Vector2 b = 3 * (p0 - 2 * cp1 + cp2);
            Vector2 c = 3 * (-p0 + cp1);
            Vector2 d = p0;

            Maths.SolveQuadratic(3 * a.X, 2 * b.X, c.X, out float tx1, out float tx2);
            Maths.SolveQuadratic(3 * a.Y, 2 * b.Y, c.Y, out float ty1, out float ty2);

            bounds.X = Math.Min(bounds.X, Math.Min(p0.X, p.X));
            bounds.Y = Math.Min(bounds.Y, Math.Min(p0.Y, p.Y));
            bounds.Z = Math.Max(bounds.Z, Math.Max(p0.X, p.X));
            bounds.W = Math.Max(bounds.W, Math.Max(p0.Y, p.Y));

            if (tx1 > 0.0f && tx1 < 1.0f)
            {
                float extremeX = Maths.InterpolateCubicBezier(p0.X, cp1.X, cp2.X, p.X, tx1);
                bounds.X = Math.Min(bounds.X, extremeX);
                bounds.Z = Math.Max(bounds.Z, extremeX);
            }
            if (tx2 > 0.0f && tx2 < 1.0f)
            {
                float extremeX = Maths.InterpolateCubicBezier(p0.X, cp1.X, cp2.X, p.X, tx2);
                bounds.X = Math.Min(bounds.X, extremeX);
                bounds.Z = Math.Max(bounds.Z, extremeX);
            }
            if (ty1 > 0.0f && ty1 < 1.0f)
            {
                float extremeY = Maths.InterpolateCubicBezier(p0.Y, cp1.Y, cp2.Y, p.Y, ty1);
                bounds.Y = Math.Min(bounds.Y, extremeY);
                bounds.W = Math.Max(bounds.W, extremeY);
            }
            if (ty2 > 0.0f && ty2 < 1.0f)
            {
                float extremeY = Maths.InterpolateCubicBezier(p0.Y, cp1.Y, cp2.Y, p.Y, ty2);
                bounds.Y = Math.Min(bounds.Y, extremeY);
                bounds.W = Math.Max(bounds.W, extremeY);
            }
            
            scene.FlagAddingCubic();
            scene.AddPoint(cp1, cp2, p);

            return p;
        }
    }
}