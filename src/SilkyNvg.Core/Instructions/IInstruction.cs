using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal interface IInstruction
    {

        void BuildPaths(PixelRatio pixelRatio, PathCache pathCache);

    }
}
