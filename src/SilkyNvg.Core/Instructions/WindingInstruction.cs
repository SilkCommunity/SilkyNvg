using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using SilkyNvg.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct WindingInstruction : IInstruction
    {

        private readonly Winding _winding;

        public WindingInstruction(Winding winding)
        {
            _winding = winding;
        }

        public void BuildPaths(PixelRatio pixelRatio, PathCache pathCache)
        {
            pathCache.LastPath.Winding = _winding;
        }

    }
}
