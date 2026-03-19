using System.Numerics;

namespace SilkyNvg.Commands
{
    internal readonly struct QuadraticCurveToCommand : ICommand
    {

        private readonly Vector2 _cp;
        private readonly Vector2 _p;

        internal QuadraticCurveToCommand(Vector2 p, Vector2 cp)
        {
            _cp = cp;
            _p = p;
        }

    }
}