using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core.Instructions
{
    internal class CloseInstruction : IInstruction
    {

        public InstructionType Type => InstructionType.Close;
        public float[] Data => new float[] { (float)Type };

        public void Setup(State _) { }

        public void Execute(PathCache cache, Style _)
        {
            cache.ClosePath();
        }

    }
}