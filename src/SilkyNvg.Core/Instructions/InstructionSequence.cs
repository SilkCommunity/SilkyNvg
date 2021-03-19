using Silk.NET.Maths;
using SilkyNvg.Paths;
using System;

namespace SilkyNvg.Core.Instructions
{
    internal class InstructionSequence : IDisposable
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

        public Vector2D<float> GetXY()
        {
            var list = new System.Collections.Generic.List<float>();
            if (_instructionCount > 1)
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

        public void AddBezireTo(float p0x, float p0y, float p1x, float p1y, float p2x, float p2y)
        {
            Add(new BezierToInstruction(p0x, p0y, p1x, p1y, p2x, p2y));
        }

        public void AddClose()
        {
            Add(new CloseInstruction());
        }

        public void AddWinding(Winding direction)
        {
            Add(new WindingInstruction(direction));
        }

        public void Dispose()
        {
            _instructions = null;
        }

    }
}