using System;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct QuadraticCurveToCommand : ICommand
    {

        private readonly Vector2 _cp;
        private readonly Vector2 _p;

        internal QuadraticCurveToCommand(Vector2 p, Vector2 cp)
        {
            _cp = cp;
            _p = p;
        }

        public Vector2 FillToScene(Matrix3x2 transform, Vector2 p0, Vector2 subpathFirstPoint, ref Vector4 bounds, Scene scene, RenderTolerances tol)
        {
            Vector2 cp = Vector2.Transform(_cp, transform);
            Vector2 p = Vector2.Transform(_p, transform);
            
            // Bounds
            Vector2 a = p0 - 2 * cp + p;
            Vector2 b = 2 * (-p0 + cp);

            // Just add some already checked values here. 
            // If the tests below fail, then this means doing nothing.
            float extremeX = p0.X;
            float extremeY = p0.Y;

            if (Math.Abs(a.X) >= tol.FloatingPointTol)
            {
                float t = -b.X / (2 * a.X);
                if (t > 0.0f && t < 1.0f)
                {
                    extremeX = Maths.InterpolateQuadraticBezier(p0.X, cp.X, p.X, t);
                }
            }

            if (Math.Abs(a.Y) >= tol.FloatingPointTol)
            {
                float t = -b.Y / (2 * a.Y);
                if (t > 0.0f && t < 1.0f)
                {
                    extremeY = Maths.InterpolateQuadraticBezier(p0.Y, cp.Y, p.Y, t);
                }
            }

            bounds.X = Math.Min(Math.Min(bounds.X, p0.X), Math.Min(extremeX, p.X));
            bounds.Y = Math.Min(Math.Min(bounds.Y, p0.Y), Math.Min(extremeY, p.Y));
            bounds.Z = Math.Max(Math.Max(bounds.Z, p0.X), Math.Max(extremeX, p.X));
            bounds.W = Math.Max(Math.Max(bounds.W, p0.Y), Math.Max(extremeY, p.Y));

            scene.AddQuadraticPoints(cp, p);
            
            return p;
        }
    }
}