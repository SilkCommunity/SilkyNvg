using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal readonly struct MoveToCommand : ICommand
    {
        
        private readonly Vector2 _p;
        
        internal MoveToCommand(Vector2 p)
        {
            _p = p;
        }

        public Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol)
        {
            frame.BeginSubpath(_p);

            return _p;
        }
        
    }
}