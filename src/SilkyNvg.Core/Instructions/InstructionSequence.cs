using System;

namespace SilkyNvg.Core.Instructions
{
    public class InstructionSequence : IDisposable
    {

        private IInstruction[] _instructions;
        private int _instructionCount;

        public IInstruction[] Instructions => _instructions;
        public int Length => _instructionCount;

        public InstructionSequence(int instructionCount)
        {
            _instructions = new IInstruction[instructionCount];
            _instructionCount = 0;
        }

        public IInstruction this[int index]
        {
            get => _instructions[index];
        }

        public void Add(IInstruction instruction)
        {
            _instructions[_instructionCount++] = instruction;
        }

        // TODO: Implement the following methods:
        public void AddMoveTo()
        {

        }

        public void AddLineTo()
        {

        }

        public void AddBezireTo()
        {

        }

        public void AddClose()
        {

        }

        public void AddWinding()
        {

        }

        public void Dispose()
        {
            _instructions = null;
        }

    }
}
