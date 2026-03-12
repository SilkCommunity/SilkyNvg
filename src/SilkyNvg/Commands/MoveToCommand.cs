using System.Numerics;

namespace SilkyNvg.Commands
{
    internal readonly struct MoveToCommand : ICommand
    {
        
        private readonly Vector2 _p;

        public Vector2 EndPoint => _p;
        
        internal MoveToCommand(Vector2 p)
        {
            _p = p;
        }

    }
}