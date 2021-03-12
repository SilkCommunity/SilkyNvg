using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core.Instructions
{
    internal interface IInstruction
    {

        bool RequiresPosition { get; }

        InstructionType Type { get; }

        float[] Data { get; }

        void Prepare(State state);

        void Execute(PathCache cache, Style style);

    }
}