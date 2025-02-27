using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct MoveToInstruction : IInstruction
    {

        private readonly Vector2D<float> _position;

        public MoveToInstruction(Vector2D<float> position)
        {
            _position = position;
        }

        public void BuildPaths(Matrix3X2<float> transform, PixelRatio pixelRatio, PathCache pathCache)
        {
            pathCache.AddPath(pixelRatio);
            pathCache.LastPath.AddPoint(Vector2D.Transform(_position, transform), PointFlags.Corner);
        }

    }
}
