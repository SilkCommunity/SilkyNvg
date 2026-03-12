using System.Numerics;

namespace SilkyNvg.Commands
{
    internal readonly struct LineToCommand : ICommand
    {
        
        private readonly Vector2 _p;

        public Vector2 EndPoint => _p;
        
        internal LineToCommand(Vector2 p)
        {
            _p = p;
        }

    }
}