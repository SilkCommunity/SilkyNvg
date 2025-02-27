using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;

namespace SilkyNvg.Core.Instructions
{
    internal interface IInstruction
    {

        void BuildPaths(Matrix3X2<float> transform, PixelRatio pixelRatio, PathCache pathCache);

    }
}
