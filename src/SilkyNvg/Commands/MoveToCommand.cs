using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal readonly struct MoveToCommand : ICommand
    {
        
        private readonly Vector2 _p;
        
        internal MoveToCommand(Vector2 p)
        {
            _p = p;
        }

        public void Fill(FrameContainer frame)
        {
            frame.BeginSubpath(_p);
        }
        
    }
}