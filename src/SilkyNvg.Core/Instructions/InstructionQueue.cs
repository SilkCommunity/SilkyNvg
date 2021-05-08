using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal class InstructionQueue
    {

        private readonly Queue<IInstruction> _instructions;

        public InstructionQueue()
        {
            _instructions = new();
        }

        public void AddInstruction(IInstruction instruction)
        {
            _instructions.Enqueue(instruction);
        }

        public void BuildPaths()
        {
            // TODO: Build paths
            _instructions.Clear();
        }

    }
}
