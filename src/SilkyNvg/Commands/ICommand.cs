using System.Numerics;
using SilkyNvg.Rendering;

namespace SilkyNvg.Commands
{
    public interface ICommand
    {

        void Fill(FrameContainer frame);

    }
}