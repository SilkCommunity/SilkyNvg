using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core.Instructions
{
    internal interface IInstruction
    {

        InstructionType Type { get; }

        float[] Data { get; }

        void Setup(State state);

        void Execute(PathCache cache, Style style);

    }
}