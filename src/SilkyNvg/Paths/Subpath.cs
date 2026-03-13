using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Rendering;

namespace SilkyNvg.Paths
{
    internal class Subpath
    {
        
        private readonly List<ICommand> _commands = new List<ICommand>();

        internal bool IsClosed { get; private set; }
        
        internal Vector2 FirstPoint { get; }
        
        internal Subpath(Vector2 firstPoint)
        {
            IsClosed = false;
            FirstPoint = firstPoint;
            _commands.Add(new MoveToCommand(firstPoint));
        }

        internal void AddCommand(ICommand command)
        {
            _commands.Add(command);
        }

        internal void Fill(FrameContainer frame, RenderTolerances tolerances)
        {
            if (_commands.Count <= 1)
            {
                return;
            }

            Vector2 p0 = Vector2.Zero;
            foreach (var command in _commands)
            {
                p0 = command.Fill(Matrix3x2.Identity, p0, frame, tolerances);
            }
            
            // Subpath is begun in MoveToCommand fill call.
            frame.EndSubpath();
        }

        internal void Close()
        {
            _commands.Add(new CloseCommand());
            IsClosed = true;
        }
        
    }
}