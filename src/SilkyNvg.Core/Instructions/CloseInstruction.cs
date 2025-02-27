using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct CloseInstruction : IInstruction
    {

        public CloseInstruction() { }

        public void BuildPaths(PixelRatio pixelRatio, PathCache pathCache)
        {
            pathCache.LastPath.Close();
        }

    }
}
