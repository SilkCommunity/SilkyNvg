using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using System.Numerics;

namespace SilkyNvg.Core.Instructions
{
    internal class MoveToInstruction : IInstruction
    {
        private readonly Vector2 _position;
        private readonly PathCache _pathCache;

        public MoveToInstruction(Vector2 position, PathCache pathCache)
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
