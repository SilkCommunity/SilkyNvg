using Silk.NET.Maths;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    public sealed class InstructionManager
    {

        public const int INITIAL_INSTRUCTIONS_SIZE = 256;

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

        private Queue<IInstruction> _instructionQueue;
        private Vector2D<float> _instructionPosition;
        private int _instructionQueueCapacity;

        public InstructionManager()
        {
            _instructionQueue = new Queue<IInstruction>(INITIAL_INSTRUCTIONS_SIZE);
            _instructionQueue.Clear();
            _instructionQueueCapacity = INITIAL_INSTRUCTIONS_SIZE;
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
            if (_instructionQueue.Count + sequence.Length > _instructionQueueCapacity)
            {
                int newCap = _instructionQueue.Count + sequence.Length + _instructionQueueCapacity / 2;
                var cache = _instructionQueue.ToArray();
                _instructionQueue = new Queue<IInstruction>(newCap);
                foreach (var instruction in cache)
                    _instructionQueue.Enqueue(instruction);
                _instructionQueueCapacity = newCap;
            }

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

    }
}
