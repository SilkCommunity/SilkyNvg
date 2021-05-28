using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal class MoveToInstruction : IInstruction
    {

        private readonly Vector2D<float> _position;
        private readonly PathCache _pathCache;

        public MoveToInstruction(Vector2D<float> position, PathCache pathCache)
        {
            _position = position;
            _pathCache = pathCache;
        }

        public void BuildPaths()
        {
            _pathCache.AddPath();
            _pathCache.LastPath.AddPoint(_position, PointFlags.Corner);
        }

    }
}
