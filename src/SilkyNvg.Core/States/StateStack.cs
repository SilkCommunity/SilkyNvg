using SilkyNvg.Core.Maths;
using System.Collections.Generic;

namespace SilkyNvg.Core.States
{
    internal class StateStack
    {

        private const byte MAX_STATES = 32;

        private readonly Stack<State> _states;

        public State CurrentState
        {
            get
            {
                return _states.Peek();
            }
        }

        public StateStack()
        {
            _states = new();
        }

        public void Save()
        {
            if (_states.Count >= MAX_STATES)
            {
                return;
            }

            if (_states.Count > 0)
            {
                _states.Push(_states.Peek());
            } else if (_states.Count == 0)
            {
                _states.Push(default);
            }
        }

        public void Restore()
        {
            if (_states.Count <= 1)
            {
                return;
            }

            _states.Pop();
        }

        public void Reset()
        {
            var state = CurrentState;

            state.fill = new Paint(new Colour(255, 255, 255, 255));
            state.stroke = new Paint(new Colour(0, 0, 0, 255));
            state.compositeOperation = new CompositeOperationState(CompositeOperation.SourceOut);
            state.shapeAntialias = true;
            state.strokeWidth = 1.0f;
            state.miterLimit = 10.0f;
            state.lineCap = LineCap.Butt;
            state.lineJoin = LineCap.Miter;
            state.alpha = 1.0f;
            state.transform = TransformsImpl.Identity;

            state.scissor = new();

            // TODO: Font
        }

    }
}
