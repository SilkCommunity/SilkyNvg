using Silk.NET.Maths;
using SilkyNvg.Core;
using SilkyNvg.Core.Geometry;
using SilkyNvg.Paths;
using SilkyNvg.States;
using System;

namespace SilkyNvg.Instructions
{
    internal sealed class MoveToInstruction : IInstruction
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

        public void FlattenPath(PathCache cache, Style _)
        {
            cache.AddPath();
            cache.AddPoint(_position, (uint)PointFlags.PointCorner);
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }

    }
}
