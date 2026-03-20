using System;
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

        public Vector2 FillToScene(Matrix3x2 transform, Vector2 p0, Vector2 subpathFirstPoint, ref Vector4 bounds, Scene scene, RenderTolerances tol)
        {
            // Get transformed points
            Vector2 p = Vector2.Transform(_p, transform);
            
            // Bounds
            bounds.X = Math.Min(bounds.X, p.X);
            bounds.Y = Math.Min(bounds.Y, p.Y);
            bounds.Z = Math.Max(bounds.Z, p.X);
            bounds.W = Math.Max(bounds.W, p.Y);
            
            scene.AddLinePoint(p);
            return p;
        }
    }
}