using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal readonly struct QuadraticCurveToCommand : ICommand
    {

        private readonly Vector2 _cp;
        private readonly Vector2 _p;
        
        internal QuadraticCurveToCommand(Vector2 cp, Vector2 p)
        {
            _cp = cp;
            _p = p;
        }

        public void Fill(FrameContainer frame)
        {
            frame.AddCommand(CommandType.QuadraticCurveTo);
            frame.AddPoint(_cp, _p);
        }
        
    }
}