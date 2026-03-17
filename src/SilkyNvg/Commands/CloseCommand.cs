using System;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct CloseCommand : ICommand
    {
        
        public Vector2 Fill(Matrix3x2 transform, Vector2 p0, FrameContainer frame, RenderTolerances tol)
        {
            Vector2 pathStart = frame.SubpathFirstPoint;
            if (!pathStart.FpEquals(p0, tol.FloatingPointTol))
            {
                var cmd = new LineToCommand(pathStart);
                cmd.Fill(transform, p0, frame, tol);
            }

            frame.MarkSubpathClosed();
            return pathStart;
        }
        
    }
}