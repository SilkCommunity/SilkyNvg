using System.Numerics;

namespace SilkyNvg.Commands
{
    internal readonly struct QuadraticCurveToCommand : ICommand
    {

        private readonly Vector2 _cp;
        private readonly Vector2 _p;

        public Vector2 EndPoint => _p;
        
        internal QuadraticCurveToCommand(Vector2 cp, Vector2 p)
        {
            _cp = cp;
            _p = p;
        }

    }
}