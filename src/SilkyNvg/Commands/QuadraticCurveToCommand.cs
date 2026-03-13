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

            CutToLength(p0, cp, p, frame, tol);

            return p;
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

            Maths.CutQuadraticBezierInHalf(p0, cp, p1, out Vector2 cpl, out Vector2 cpr, out Vector2 hp);
            CutToLength(p0, cpl, hp, frame, tol);
            CutToLength(hp, cpr, p1, frame, tol);
        }
        
    }
}