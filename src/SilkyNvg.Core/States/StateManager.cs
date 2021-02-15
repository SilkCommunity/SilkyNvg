using System.Collections.Generic;

namespace SilkyNvg.Core.States
{
    public sealed class StateManager
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
            var state = new State();

            // TODO: Reset state default values.

            if (_stateStack.Count > 0)
                _stateStack.RemoveLast();
            _stateStack.AddLast(state);
        }

        public State GetState()
        {
            return _stateStack.Last.Value;
        }

    }
}
