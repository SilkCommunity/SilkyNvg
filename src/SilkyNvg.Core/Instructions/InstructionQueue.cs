using Silk.NET.Maths;
using System.Collections.Generic;

namespace SilkyNvg.Core.Instructions
{
    internal sealed class InstructionQueue
    {

        private readonly Nvg _nvg;

        private readonly Queue<IInstruction> _instructions = new();

        public InstructionQueue(Nvg nvg)
        {
            _nvg = nvg;
        }

        public void AddMoveTo(Vector2D<float> pos)
        {
            _instructions.Enqueue(new MoveToInstruction(Vector2D.Transform(pos, _nvg.stateStack.CurrentState.Transform), _nvg.pathCache));
        }

        public void AddLineTo(Vector2D<float> pos)
        {
            _instructions.Enqueue(new LineToInstruction(Vector2D.Transform(pos, _nvg.stateStack.CurrentState.Transform), _nvg.pathCache));
        }

        public void AddClose()
        {
            _instructions.Enqueue(new CloseInstruction(_nvg.pathCache));
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
