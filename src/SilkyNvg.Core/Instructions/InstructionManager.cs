using Silk.NET.Maths;
using SilkyNvg.Core.States;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal sealed class InstructionManager
    {

        public Vector2D<float> InstructionPosition
        {
            get => _instructionPosition;
            set => _instructionPosition = value;
        }

        public float InstructionX
        {
            get => _instructionPosition.X;
            set => _instructionPosition.X = value;
        }

        public float InstructionY
        {
            get => _instructionPosition.Y;
            set => _instructionPosition.Y = value;
        }

        public int QueueLength => _instructionQueue.Count;

        private readonly Queue<IInstruction> _instructionQueue;

        private Vector2D<float> _instructionPosition;

        public InstructionManager()
        {
            _instructionQueue = new Queue<IInstruction>();
            _instructionPosition = new Vector2D<float>();
        }

        public IInstruction Next()
        {
            return _instructionQueue.Dequeue();
        }

        public IInstruction QueueAt(int index)
        {
            return _instructionQueue.ToArray()[index];
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
                instruction.Prepare(state);
                EnqueueInstruction(instruction);
            }
        }

        public void Clear()
        {
            _instructionQueue.Clear();
        }

    }
}