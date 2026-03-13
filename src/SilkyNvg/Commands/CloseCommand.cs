using System;
using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal readonly struct CloseCommand : ICommand
    {
        
        public Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol)
        {
            frame.CloseSubpath();
            return default;
        }
        
    }
}