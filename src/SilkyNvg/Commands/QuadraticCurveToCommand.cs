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
        
        internal QuadraticCurveToCommand(Vector2 cp, Vector2 p)
        {
            _cp = cp;
            _p = p;
        }
        
        public Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol)
        {
            Vector2 cp = Vector2.Transform(_cp, transform);
            Vector2 p = Vector2.Transform(_p, transform);

            MonotoniseAndCutToLength(p0, cp, p, frame, tol);

            return p;
        }

        private static void MonotoniseAndCutToLength(Vector2 p0, Vector2 cp, Vector2 p, FrameContainer frame, RenderTolerances tol)
        {
            Vector2 a = p0 - 2 * cp + p;
            Vector2 b = 2 * (-p0 + cp);
            Vector2 c = p0;

            float tx= float.PositiveInfinity;
            float ty = float.PositiveInfinity;
            if (Math.Abs(a.X) >= tol.FloatingPointTol)
            {
                float t = -b.X / (2 * a.X);
                if (t > 0.0f && t < 1.0f)
                {
                    tx = t;
                }
            }
            if (Math.Abs(a.Y) >= tol.FloatingPointTol)
            {
                float t = -b.Y / (2 * a.Y);
                if (t > 0.0f && t < 1.0f)
                {
                    ty = t;
                }
            }

            // Manual insertion sort
            float t0 = Math.Min(tx, ty);
            
            // Add segments
            if (float.IsPositiveInfinity(t0))
            {
                CutToLength(p0, cp, p, frame, tol);
            }
            else
            {
                Maths.CutQuadraticBezier(p0, cp, p, t0, out Vector2 cpl, out Vector2 cpr, out Vector2 hp);
                CutToLength(p0, cpl, hp, frame, tol);
                MonotoniseAndCutToLength(hp, cpr, p, frame, tol);
            }
        }

        private static void CutToLength(Vector2 p0, Vector2 cp, Vector2 p1, FrameContainer frame, RenderTolerances tol)
        {
            float upperLengthBound = (cp - p0).Length() + (p1 - cp).Length(); // Triangle inequality

            if (upperLengthBound <= tol.MaxPathLength)
            {
                frame.AddCommand(CommandType.QuadraticCurveTo);
                frame.AddPoint(cp, p1);
                return;
            }

            Maths.CutQuadraticBezier(p0, cp, p1, 0.5f, out Vector2 cpl, out Vector2 cpr, out Vector2 hp);
            CutToLength(p0, cpl, hp, frame, tol);
            CutToLength(hp, cpr, p1, frame, tol);
        }
        
    }
}