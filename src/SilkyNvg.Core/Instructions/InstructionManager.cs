using Silk.NET.Maths;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    public sealed class InstructionManager
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

        public void AddSequence(InstructionSequence sequence)
        {
            if (sequence.RequiresPosition)
            {
                _instructionPosition = sequence.Position;
            }

            for (int i = 0; i < sequence.Length; i++)
            {
                var instruction = sequence[i];
                instruction.Prepare();
                EnqueueInstruction(instruction);
            }
            sequence.Dispose();
        }

        public void Clear()
        {
            _instructionQueue.Clear();
        }

    }
}
