using SilkyNvg.Paths;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Core.Instructions
{
    internal sealed class InstructionQueue
    {
        private const uint INIT_INSTRUCTIONS_SIZE = 256;

        private readonly Queue<IInstruction> _instructions = new((int)INIT_INSTRUCTIONS_SIZE);

        private readonly Nvg _nvg;

        public Vector2 EndPosition { get; private set; }

        public uint Count => (uint)_instructions.Count;

        public InstructionQueue(Nvg nvg)
        {
            _nvg = nvg;
            EndPosition = default;
        }

        public void AddMoveTo(Vector2 pos)
        {
            EndPosition = pos;
            _instructions.Enqueue(new MoveToInstruction(Vector2.Transform(pos, _nvg.stateStack.CurrentState.Transform), _nvg.pathCache));
        }

        public void AddLineTo(Vector2 pos)
        {
            EndPosition = pos;
            _instructions.Enqueue(new LineToInstruction(Vector2.Transform(pos, _nvg.stateStack.CurrentState.Transform), _nvg.pathCache));
        }

        public void AddBezierTo(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            EndPosition = p2;
            Matrix3x2 transform = _nvg.stateStack.CurrentState.Transform;
            _instructions.Enqueue(new BezierToInstruction(Vector2.Transform(p0, transform), Vector2.Transform(p1, transform), Vector2.Transform(p2, transform),
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
            if (_nvg.pathCache.Paths.Count > 0)
            {
                return;
            }

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
