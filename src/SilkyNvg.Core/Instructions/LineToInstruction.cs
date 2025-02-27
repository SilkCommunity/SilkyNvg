using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct LineToInstruction : IInstruction
    {

        private readonly Vector2D<float> _position;

        public LineToInstruction(Vector2D<float> position, Matrix3X2<float> transform)
        {
            _position = Vector2D.Transform(position, transform);
        }

        public void BuildPaths(PixelRatio pixelRatio, PathCache pathCache)
        {
            pathCache.LastPath.AddPoint(_position, PointFlags.Corner);
        }

    }
}
