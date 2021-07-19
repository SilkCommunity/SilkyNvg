using SilkyNvg.Core.Paths;
using SilkyNvg.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct WindingInstruction : IInstruction
    {

        private readonly Winding _winding;
        private readonly PathCache _pathCache;

        public WindingInstruction(Winding winding, PathCache pathCache)
        {
            _winding = winding;
            _pathCache = pathCache;
        }

        public void BuildPaths()
        {
            _pathCache.LastPath.Winding = _winding;
        }

    }
}
