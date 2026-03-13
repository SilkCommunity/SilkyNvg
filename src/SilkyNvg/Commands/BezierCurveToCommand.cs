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
            frame.AddCommand(CommandType.BezierCurveTo);
            frame.AddPoint(_cp1, _cp2, _p);
            return _p;
        }

        private static void CutToLength(Vector2 p0, Vector2 cp1, Vector2 cp2, Vector2 p1, FrameContainer frame, RenderTolerances tol)
        {
            float upperLengthBound = (cp1 - p0).Length() + (cp2 - cp1).Length() + (p1 - cp2).Length(); // Triangle inequality

            if (upperLengthBound <= tol.MaxPathLength)
            {
                frame.AddCommand(CommandType.BezierCurveTo);
                frame.AddPoint(cp1, cp2, p1);
                return;
            }

            Maths.CutCubicBezierInHalf(p0, cp1, cp2, p1, out Vector2 cpl1, out Vector2 cpl2, out Vector2 cpr1,
                out Vector2 cpr2, out Vector2 hp);
            CutToLength(p0, cpl1, cpl2, hp, frame, tol);
            CutToLength(hp, cpr1, cpr2, p1, frame, tol);
        }

    }
}