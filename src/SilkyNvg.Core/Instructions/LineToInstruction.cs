using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal class LineToInstruction : IInstruction
    {
        private readonly Vector2D<float> _position;
        private readonly PathCache _pathCache;

        public LineToInstruction(Vector2D<float> position, PathCache pathCache)
        {
            _position = position;
            _pathCache = pathCache;
        }

        public void BuildPaths()
        {
            _pathCache.LastPath.AddPoint(_position, PointFlags.Corner);
        }
    }
}
