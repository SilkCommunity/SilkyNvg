using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal struct CloseInstruction : IInstruction
    {

        public CloseInstruction() { }

        public void BuildPaths(Matrix3X2<float> transform, PixelRatio pixelRatio, PathCache pathCache)
        {
            pathCache.LastPath.Close();
        }

    }
}
