using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal readonly struct LineToCommand : ICommand
    {
        
        private readonly Vector2 _p;
        
        internal LineToCommand(Vector2 p)
        {
            _p = p;
        }

        public Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol)
        {
            Vector2 p = Vector2.Transform(_p, transform);
            Vector2 d = p - p0;
            
            float length = d.Length();
            int numSegments = (int)(length / tol.MaxPathLength) + 1;
            d /= numSegments;
            
            for (int i = 1; i <= numSegments; i++)
            {
                frame.AddCommand(CommandType.LineTo);
                frame.AddPoint(p0 + i * d);
            }

            return p;
        }
        
    }
}