using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilkyNvg.Core.Instructions
{
    public class MoveToInstruction : IInstruction
    {

        private readonly Vector2D<float> _position;

        public bool RequiresPosition => true;

        public Vector2D<float> Position => _position;

        internal MoveToInstruction(float x, float y)
        {
            _position = new Vector2D<float>(x, y);
        }

        public void Prepare()
        {

        }

        public void Execute()
        {

        }

    }
}
