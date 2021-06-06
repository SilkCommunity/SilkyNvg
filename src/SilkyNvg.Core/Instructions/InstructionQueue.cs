using Silk.NET.Maths;
using SilkyNvg.Paths;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal sealed class InstructionQueue
    {

        private readonly Nvg _nvg;

        private readonly Queue<IInstruction> _instructions = new();

        public Vector2D<float> EndPosition { get; private set; }

        public uint Count => (uint)_instructions.Count;

        public InstructionQueue(Nvg nvg)
        {
            _nvg = nvg;
            EndPosition = default;
        }

        public void AddMoveTo(Vector2D<float> pos)
        {
            EndPosition = pos;
            _instructions.Enqueue(new MoveToInstruction(Vector2D.Transform(pos, _nvg.stateStack.CurrentState.Transform), _nvg.pathCache));
        }

        public void AddLineTo(Vector2D<float> pos)
        {
            EndPosition = pos;
            _instructions.Enqueue(new LineToInstruction(Vector2D.Transform(pos, _nvg.stateStack.CurrentState.Transform), _nvg.pathCache));
        }

        public void AddBezierTo(Vector2D<float> p0, Vector2D<float> p1, Vector2D<float> p2)
        {
            EndPosition = p2;
            Matrix3X2<float> transform = _nvg.stateStack.CurrentState.Transform;
            _instructions.Enqueue(new BezierToInstruction(Vector2D.Transform(p0, transform), Vector2D.Transform(p1, transform), Vector2D.Transform(p2, transform),
                _nvg.pixelRatio.TessTol, _nvg.pathCache));
        }

        public void AddClose()
        {
            _instructions.Enqueue(new CloseInstruction(_nvg.pathCache));
        }

        public void AddWinding(Winding winding)
        {
            _instructions.Enqueue(new WindingInstruction(winding, _nvg.pathCache));
        }

        public void FlattenPaths()
        {
            while (_instructions.Count > 0)
            {
                _instructions.Dequeue().BuildPaths();
            }
            _nvg.pathCache.FlattenPaths();
        }

        public void Clear()
        {
            _instructions.Clear();
        }

    }
}
