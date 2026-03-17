using System;
using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

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

            Vector2 prev = p0;
            for (int i = 1; i <= numSegments; i++)
            {
                Vector2 p1 = p0 + i * d;

                Vector2 topLeft = Vector2.Min(prev, p1);
                Vector2 bottomRight = Vector2.Max(prev, p1);
                
                // Double cast necessary because we want rounding down both before subtracting
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
                
                frame.AddCommand(nIntX, nIntY, CommandType.LineTo);
                frame.AddPoint(p0 + i * d);

                prev = p1;
            }

            return p;
        }
        
    }
}