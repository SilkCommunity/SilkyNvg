using Silk.NET.Maths;
using SilkyNvg.Core.States;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal class InstructionQueue
    {

        private readonly Queue<IInstruction> _instructionQueue;

        private Vector2D<float> _instructionPosition;

        public Vector2D<float> InstructionPosition
        {
            get => _instructionPosition;
            set => _instructionPosition = value;
        }

        public int QueueLength => _instructionQueue.Count;

        public InstructionQueue()
        {
            _instructionQueue = new Queue<IInstruction>();
            _instructionPosition = new Vector2D<float>();
        }

        public IInstruction Next()
        {
            return _instructionQueue.Dequeue();
        }

        private void EnqueueInstruction(IInstruction instruction)
        {
            _instructionQueue.Enqueue(instruction);
        }

        public void AddSequence(InstructionSequence sequence, State state)
        {
            if (sequence[0].Type != InstructionType.Close && sequence[0].Type != InstructionType.Winding)
            {
                _instructionPosition = sequence.GetXY();
            }

            foreach (IInstruction instruction in sequence.Instructions)
            {
                instruction.Setup(state);
                EnqueueInstruction(instruction);
            }
        }

        public void Clear()
        {
            _instructionQueue.Clear();
        }

    }
}