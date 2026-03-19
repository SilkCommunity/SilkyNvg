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
            _commands.Add(new MoveToCommand(p));
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
            var bounds = new Vector4(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity,
                float.NegativeInfinity);
            
            // Ignore subpaths with <= 1 point (see: HTML5 Canvas API Spec)
            if (_commands.Count <= 1)
            {
                return bounds;
            }
            
            Vector2 firstPoint = Vector2.Transform(FirstPoint, transform);
            
            Vector2 p0 = default;
            foreach (var cmd in _commands)
            {
                p0 = cmd.FillToScene(transform, p0, firstPoint, ref bounds, scene, tolerances);
            }

            return bounds;
        }
        
    }
}