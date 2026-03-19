using System.Numerics;

namespace SilkyNvg.Commands
{
    internal readonly struct BezierCurveToCommand : ICommand
    {

        private readonly Vector2 _cp1;
        private readonly Vector2 _cp2;
        private readonly Vector2 _p;

        internal BezierCurveToCommand(Vector2 cp1, Vector2 cp2, Vector2 p)
        {
            _cp1 = cp1;
            _cp2 = cp2;
            _p = p;
        }

    }
}