using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.Paths;
using SilkyNvg.Paths;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal sealed class InstructionQueue
    {

        private const uint INIT_INSTRUCTIONS_SIZE = 256;

        private readonly Queue<IInstruction> _instructions = new((int)INIT_INSTRUCTIONS_SIZE);

        public Vector2D<float> EndPosition { get; private set; }

        public uint Count => (uint)_instructions.Count;

        public InstructionQueue()
        {
            EndPosition = default;
        }

        public void AddMoveTo(Vector2D<float> pos, Matrix3X2<float> transform)
        {
            EndPosition = pos;
            _instructions.Enqueue(new MoveToInstruction(pos, transform));
        }

        public void AddLineTo(Vector2D<float> pos, Matrix3X2<float> transform)
        {
            EndPosition = pos;
            _instructions.Enqueue(new LineToInstruction(pos, transform));
        }

        public void AddBezierTo(Vector2D<float> p0, Vector2D<float> p1, Vector2D<float> p2, Matrix3X2<float> transform)
        {
            EndPosition = p2;
            _instructions.Enqueue(new BezierToInstruction(p0, p1, p2, transform));
        }

        public void AddClose()
        {
            _instructions.Enqueue(new CloseInstruction());
        }

        public void AddWinding(Winding winding)
        {
            _instructions.Enqueue(new WindingInstruction(winding));
        }

        public void FlattenPaths(PixelRatio pixelRatio, PathCache pathCache)
        {
            while (_instructions.Count > 0)
            {
                _instructions.Dequeue().BuildPaths(pixelRatio, pathCache);
            }
            pathCache.FlattenPaths();
        }

        public void Clear()
        {
            _instructions.Clear();
        }

    }
}
