using Silk.NET.Maths;
using System;

namespace SilkyNvg.Core.Instructions
{
    public class InstructionSequence : IDisposable
    {

        private IInstruction[] _instructions;
        private int _instructionCount;

        private Vector2D<float> _position;
        private bool _positioned = false;

        public IInstruction[] Instructions => _instructions;
        public int Length => _instructionCount;
        public Vector2D<float> Position => _position;
        public bool RequiresPosition => _positioned;

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

        public Vector2D<float> GetXY()
        {
            var list = new System.Collections.Generic.List<float>();
            list.AddRange(_instructions[_instructionCount - 2].Data);
            list.AddRange(_instructions[_instructionCount - 1].Data);
            return new Vector2D<float>(list[^2], list[^1]);
        }

        // TODO: Implement the following methods:
        public void AddMoveTo(float x, float y)
        {
            Add(new MoveToInstruction(x, y));
        }

        public void AddLineTo(float x, float y)
        {
            Add(new LineToInstruction(x, y));
        }

        public void AddBezireTo()
        {

        }

        public void AddClose()
        {
            Add(new CloseInstruction());
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
