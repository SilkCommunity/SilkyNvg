using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;

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

        internal void Close()
        {
            _commands.Add(new CloseCommand());
            IsClosed = true;
        }
        
    }
}