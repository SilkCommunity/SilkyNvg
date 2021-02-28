using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;

namespace SilkyNvg.Core.Instructions
{
    public class LineToInstruction : IInstruction
    {
        public bool RequiresPosition => true;
        public InstructionType Type => InstructionType.LineTo;
        public float[] Data => new float[] { (float)Type, _position.X, _position.Y };

        private Vector2D<float> _position;

        public Vector2D<float> Position => _position;

        internal LineToInstruction(float x, float y)
        {
            _position = new Vector2D<float>(x, y);
        }

        public void Prepare(State state)
        {
            _position = Maths.TransformPoint(_position, state.Transform);
        }

        public void Execute(PathCache cache, Style style)
        {
            cache.AddPoint(_position.X, _position.Y, (uint)PointFlags.PointCorner, style);
        }

    }
}
