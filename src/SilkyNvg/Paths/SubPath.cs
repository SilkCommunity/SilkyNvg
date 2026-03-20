using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Rendering;

namespace SilkyNvg.Paths
{
    internal class SubPath
    {
        
        private readonly List<ICommand> _commands = new List<ICommand>();

        internal bool Closed { get; private set; }
        
        internal Vector2 FirstPoint { get; }
        
        internal SubPath(Vector2 p)
        {
            Closed = false;
            FirstPoint = p;
        }

        internal void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        internal void MarkClosed()
        {
            AddCommand(new CloseCommand());
            Closed = true;
        }

        internal Vector4 FillToScene(Scene scene, Matrix3x2 transform, RenderTolerances tolerances)
        {
            var bounds = new Vector4(FirstPoint.X, FirstPoint.Y, FirstPoint.X, FirstPoint.Y);
            
            // Ignore subpaths with <= 1 point (see: HTML5 Canvas API Spec)
            // By definition we always have at least one point, the FirstPoint.
            if (_commands.Count < 1)
            {
                return bounds;
            }
            
            Vector2 firstPoint = Vector2.Transform(FirstPoint, transform);
            
            scene.BeginSubPath(firstPoint);
            
            Vector2 p0 = firstPoint;
            foreach (var cmd in _commands)
            {
                p0 = cmd.FillToScene(transform, p0, firstPoint, ref bounds, scene, tolerances);
            }

            scene.EndSubPath();

            return bounds;
        }
        
    }
}