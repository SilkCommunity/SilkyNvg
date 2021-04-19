using Silk.NET.Maths;
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
            float x, y;
            var instruction = _instructions[^1];
            y = instruction.Data[^1];
            if (instruction.Data.Length > 1)
            {
                x = instruction.Data[^2];
            }
            else
            {
                instruction = _instructions[^2];
                x = instruction.Data[^1];
            }
            return new Vector2D<float>(x, y);
        }

        // TODO: Implement the following methods:
        public void AddMoveTo(float x, float y)
        {
            Add(new MoveToInstruction(x, y));
        }

        public void AddMoveTo(Vector2D<float> position)
        {
            Add(new MoveToInstruction(position.X, position.Y));
        }

        public void AddLineTo(float x, float y)
        {
            Add(new LineToInstruction(x, y));
        }

        public void AddLineTo(Vector2D<float> position)
        {
            Add(new LineToInstruction(position.X, position.Y));
        }

        public void AddBezierTo(float p0x, float p0y, float p1x, float p1y, float p2x, float p2y)
        {
            Add(new BezierToInstruction(p0x, p0y, p1x, p1y, p2x, p2y));
        }

        public void AddBezierTo(Vector2D<float> p0, Vector2D<float> p1, Vector2D<float> p2)
        { 
            Add(new BezierToInstruction(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y));
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