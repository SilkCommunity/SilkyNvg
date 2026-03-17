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
        
        public Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol)
        {
            Vector2 cp1 = Vector2.Transform(_cp1, transform);
            Vector2 cp2 = Vector2.Transform(_cp2, transform);
            Vector2 p = Vector2.Transform(_p, transform);

            MonotoniseAndCutToLength(p0, cp1, cp2, p, frame, tol);
            
            return p;
        }
        
        private static void MonotoniseAndCutToLength(Vector2 p0, Vector2 cp1, Vector2 cp2, Vector2 p, FrameContainer frame, RenderTolerances tol)
        {
            Vector2 a = -p0 + 3 * cp1 - 3 * cp2 + p;
            Vector2 b = 3 * (p0 - 2 * cp1 + cp2);
            Vector2 c = 3 * (-p0 + cp1);
            Vector2 d = p0;

            Maths.SolveQuadratic(3 * a.X, 2 * b.X, c.X, out float tx1, out float tx2);
            Maths.SolveQuadratic(3 * a.Y, 2 * b.Y, c.Y, out float ty1, out float ty2);

            tx1 = float.IsNaN(tx1) ? float.PositiveInfinity : tx1;
            tx2 = float.IsNaN(tx2) ? float.PositiveInfinity : tx2;
            ty1 = float.IsNaN(ty1) ? float.PositiveInfinity : ty1;
            ty2 = float.IsNaN(ty2) ? float.PositiveInfinity : ty2;

            tx1 = (tx1 <= 0.0f || tx1 >= 1.0f) ? float.PositiveInfinity : tx1;
            tx2 = (tx2 <= 0.0f || tx2 >= 1.0f) ? float.PositiveInfinity : tx2;
            ty1 = (ty1 <= 0.0f || ty1 >= 1.0f) ? float.PositiveInfinity : ty1;
            ty2 = (ty2 <= 0.0f || ty2 >= 1.0f) ? float.PositiveInfinity : ty2;
             
            // Manual insertion sort
            float t0 = Math.Min(Math.Min(tx1, tx2), Math.Min(ty1, ty2));
            
            // Add segments
            if (float.IsPositiveInfinity(t0))
            {
                CutToLength(p0, cp1, cp2, p, frame, tol);
            }
            else
            {
                Maths.CutCubicBezier(p0, cp1, cp2, p, t0, out Vector2 cpl1, out Vector2 cpl2, out Vector2 cpr1, out Vector2 cpr2, out Vector2 hp);
                CutToLength(p0, cpl1, cpl2, hp, frame, tol);
                MonotoniseAndCutToLength(hp, cpr1, cpr2, p, frame, tol);
            }
        }

        private static void CutToLength(Vector2 p0, Vector2 cp1, Vector2 cp2, Vector2 p1, FrameContainer frame, RenderTolerances tol)
        {
            float upperLengthBound = (cp1 - p0).Length() + (cp2 - cp1).Length() + (p1 - cp2).Length(); // Triangle inequality

            if (upperLengthBound <= tol.MaxPathLength)
            {
                Vector2 topLeft = Vector2.Min(p0, p1);
                Vector2 bottomRight = Vector2.Max(p0, p1);

                uint nIntX = (uint)bottomRight.X - (uint)topLeft.X;
                uint nIntY = (uint)bottomRight.Y - (uint)topLeft.Y;

                // If the last pixel lies exactly on a line, we must ignore it in our count since we always add 0 and 1 anyway.
                if (bottomRight.X.FpEquals((float)Math.Floor(bottomRight.X), tol.FloatingPointTol))
                {
                    nIntX--;
                }
                if (bottomRight.Y.FpEquals((float)Math.Floor(bottomRight.Y), tol.FloatingPointTol))
                {
                    nIntY--;
                }
                
                frame.AddCommand(nIntX, nIntY, CommandType.BezierCurveTo);
                frame.AddPoint(cp1, cp2, p1);
                return;
            }

            Maths.CutCubicBezier(p0, cp1, cp2, p1, 0.5f,  out Vector2 cpl1, out Vector2 cpl2, out Vector2 cpr1,
                out Vector2 cpr2, out Vector2 hp);
            CutToLength(p0, cpl1, cpl2, hp, frame, tol);
            CutToLength(hp, cpr1, cpr2, p1, frame, tol);
        }

    }
}