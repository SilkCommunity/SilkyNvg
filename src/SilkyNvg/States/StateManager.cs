using SilkyNvg.Core;
using System.Collections.Generic;

namespace SilkyNvg.States
{
    internal sealed class StateManager
    {

        public const int MAX_STATES = 32;

        private readonly LinkedList<State> _stateStack;

        public StateManager()
        {
            _stateStack = new LinkedList<State>();
            _stateStack.Clear();
            Reset();
        }

        public void Save()
        {
            if (_stateStack.Count > 0 && _stateStack.Count < MAX_STATES)
            {
                _stateStack.AddBefore(_stateStack.Last, _stateStack.Last.Value);
            }
        }

        public void Restore()
        {
            if (_stateStack.Count <= 1)
            {
                return;
            }
            _stateStack.RemoveLast();
        }

        public void Reset()
        {
            var state = new State
            {
                // TODO: Paint
                // TODO: Composite operations
                ShapeAntiAlias = true,
                StrokeWidth = 1.0f,
                MiterLimit = 10.0f,
                LineCap = LineCap.Butt,
                LineJoin = LineCap.Miter,
                Alpha = 1.0f,
                XForm = Maths.TransformIdentity

                // TODO: Scissor

                // TODO: Fonts
            };

            if (_stateStack.Count > 0)
                _stateStack.RemoveLast();
            _stateStack.AddLast(state);
        }

        public void ClearStack()
        {
            _stateStack.Clear();
        }

        public State GetCurrentState()
        {
            return _stateStack.Last.Value;
        }

    }
}
