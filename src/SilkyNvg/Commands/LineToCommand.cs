using System.Numerics;
using SilkyNvg.Rendering;

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

        public void Fill(FrameContainer frame)
        {
            frame.AddCommand(CommandType.LineTo);
            frame.AddPoint(_p);
        }
        
    }
}