using SilkyNvg.Core.Paths;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Core.Instructions
{
    internal class InstructionQueue
    {

        private readonly Queue<IInstruction> _instructions;

        private Vector2 _commandPosition;

        public InstructionQueue()
        {
            _instructions = new();
            _commandPosition = default;
        }

        public void AddInstructions(Matrix3x2 transform, PathCache pathCache, params IInstruction[] instructions)
        {
            if (instructions[0].RequiresPosition)
            {
                _commandPosition.X = instructions[0].Data[^2];
                _commandPosition.Y = instructions[0].Data[^1];
            }

            foreach (IInstruction instruction in instructions)
            {
                instruction.PathCache = pathCache;
                instruction.Transform(transform);
                _instructions.Enqueue(instruction);
            }
        }

        public void BuildPaths()
        {
            foreach (IInstruction instruction in _instructions)
            {
                instruction.BuildPath();
            }

            _instructions.Clear();
        }

    }
}
