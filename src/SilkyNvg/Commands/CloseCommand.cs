using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    internal readonly struct CloseCommand : ICommand
    {
        
        public void Fill(FrameContainer frame)
        {
            frame.CloseSubpath();
        }
        
    }
}