using Silk.NET.Maths;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using System;

namespace SilkyNvg.Core.Instructions
{
    public sealed class MoveToInstruction : IInstruction
    {

        public const float INSTRUCTION_ID = 0.0F;

        public bool RequiresPosition => true;

        public float ID => INSTRUCTION_ID;

        public float[] FieldsAsFloatArray
        {
            get
            {
                return new float[] { _position.X, _position.Y };
            }
        }

        private readonly State _state;

        private Vector2D<float> _position;

        public MoveToInstruction(Vector2D<float> position, State state)
        {
            _state = state;
            _position = position;
        }

        public void Prepare()
        {
            _position = Maths.TransformPoint(_position, _state.XForm);
        }

        public void FlattenPath(PathCache cache, Style style)
        {
            cache.AddPath();
            cache.AddPoint(_position, (uint)PointFlags.PointCorner, style);
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

    }
}
