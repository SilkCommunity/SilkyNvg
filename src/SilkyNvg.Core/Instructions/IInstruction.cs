using Silk.NET.Maths;

namespace SilkyNvg.Core.Instructions
{
    public interface IInstruction
    {

        bool RequiresPosition { get; }

        void Prepare();

        void Execute();

    }
}
