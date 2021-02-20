using Silk.NET.Maths;
using SilkyNvg.States;
using System;

namespace SilkyNvg.Instructions
{
    internal class InstructionSequence : IDisposable
    {

        private IInstruction[] _instructions;
        private int _instructionCount;

        private bool _positioned = false;

        public IInstruction[] Instructions => _instructions;
        public int Length => _instructionCount;
        public bool RequiresPosition => _positioned;
        public Vector2D<float> Position
        {
            get
            {
                var floatArray = ToFloatArray();
                int length = floatArray.Length;
                return new Vector2D<float>(floatArray[length - 2], floatArray[length - 1]);
            }
        }

        private readonly State _state;

        public InstructionSequence(int instructionCount, State state)
        {
            _state = state;
            _instructions = new IInstruction[instructionCount];
            _instructionCount = 0;
        }

        public IInstruction this[int index]
        {
            get => _instructions[index];
        }

        public void Add(IInstruction instruction)
        {
            if (_instructionCount == 0 && instruction.RequiresPosition)
            {
                _positioned = true;
            }

            _instructions[_instructionCount++] = instruction;
        }

        private float[] ToFloatArray()
        {
            if (_instructionCount < _instructions.Length - 1)
                throw new InvalidOperationException("Cannot convert to float array before sequence is complete!");

            var result = new System.Collections.Generic.List<float>();
            foreach (IInstruction instruction in _instructions)
            {
                result.Add(instruction.ID);
                result.AddRange(instruction.FieldsAsFloatArray);
            }

            return result.ToArray();
        }

        // TODO: Implement the following methods:
        public void AddMoveTo(float x, float y)
        {
            Add(new MoveToInstruction(new Vector2D<float>(x, y), _state));
        }

        public void AddLineTo()
        {

        }

        public void AddBezireTo(float ax, float ay, float bx, float by, float cx, float cy)
        {
            Add(new BeziereToInstruction(new Vector2D<float>(ax, ay), new Vector2D<float>(bx, by), new Vector2D<float>(cx, cy), _state));
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
