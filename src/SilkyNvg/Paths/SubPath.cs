using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;

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
            Closed = true;
        }
        
    }
}