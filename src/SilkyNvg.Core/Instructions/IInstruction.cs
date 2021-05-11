using SilkyNvg.Core.Paths;
using System.Numerics;

namespace SilkyNvg.Core.Instructions
{
    internal interface IInstruction
    {

        bool RequiresPosition { get; }

        float[] Data { get; }

        PathCache PathCache { set; }

        void Transform(Matrix3x2 transform);

        void BuildPath();

    }
}
