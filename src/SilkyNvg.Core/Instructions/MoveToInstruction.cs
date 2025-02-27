using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct MoveToInstruction : IInstruction
    {

        private readonly Vector2D<float> _position;

        public MoveToInstruction(Vector2D<float> position, Matrix3X2<float> transform)
        {
            _position = Vector2D.Transform(position, transform);
        }

        public void BuildPaths(PixelRatio pixelRatio, PathCache pathCache)
        {
            pathCache.AddPath(pixelRatio);
            pathCache.LastPath.AddPoint(_position, PointFlags.Corner);
        }

    }
}
