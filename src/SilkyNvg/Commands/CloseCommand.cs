using System.Numerics;
using SilkyNvg.Rendering;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct CloseCommand : ICommand
    {
        
        public Vector2 FillToScene(Matrix3x2 transform, Vector2 p0, Vector2 subpathFirstPoint, ref Vector4 bounds, Scene scene, RenderTolerances tol)
        {
            if (!subpathFirstPoint.FpEquals(p0, tol.FloatingPointTol))
            {
                var cmd = new LineToCommand(subpathFirstPoint);
                cmd.FillToScene(transform, p0, subpathFirstPoint, ref bounds, scene, tol);
            }

            return subpathFirstPoint;
        }
        
    }
}