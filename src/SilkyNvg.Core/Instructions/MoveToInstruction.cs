using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core.Instructions
{
    internal class MoveToInstruction : IInstruction
    {

        private Vector2D<float> _position;

        public Vector2D<float> Position => _position;
        public InstructionType Type => InstructionType.MoveTo;
        public float[] Data => new float[] { (float)Type, _position.X, _position.Y };

        public MoveToInstruction(float x, float y)
        {
            _position = new Vector2D<float>(x, y);
        }

        public MoveToInstruction(Vector2D<float> position)
        {
            _position = position;
        }

        public void Setup(State state)
        {
            _position = Maths.TransformPoint(_position, state.Transform);
        }

        public void Execute(PathCache cache, Style style)
        {
            cache.AddPath();
            cache.AddPoint(_position.X, _position.Y, (uint)PointFlags.Corner, style);
        }

    }
}