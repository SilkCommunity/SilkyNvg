using System.Collections.Generic;

namespace SilkyNvg.Core.States
{
    internal sealed class StateManager
    {

        public const int MAX_STATES = 32;

        private readonly LinkedList<State> _stateStack;

        public StateManager()
        {
            _stateStack = new LinkedList<State>();
            _stateStack.Clear();
            Save();
            Reset();
        }

        public void Save()
        {
            if (_stateStack.Count == 0)
            {
                _stateStack.AddLast(new State());
                Reset();
                return;
            }

            if (_stateStack.Count > 0 && _stateStack.Count < MAX_STATES)
            {
                State last = _stateStack.Last.Value;
                _stateStack.AddBefore(_stateStack.Last, (State)last.Clone());
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
            GetState().Reset();
        }

        public void ClearStack()
        {
            _stateStack.Clear();
        }

        public State GetState()
        {
            return _stateStack.Last.Value;
        }

    }
}