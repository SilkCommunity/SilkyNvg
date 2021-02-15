using Silk.NET.Maths;

namespace SilkyNvg.Core.Instructions
{
    public interface IInstruction
    {

        bool RequiresPosition { get; }

        Vector2D<float> Position { get; }

        void Execute();

        int PreformInitilizationPointTransforms();

    }
}
