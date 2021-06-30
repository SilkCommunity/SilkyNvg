using System.Collections.Generic;

namespace SilkyNvg.Core.States
{
    internal sealed class StateStack
    {

        private const uint MAX_STATES = 32;

        private readonly Stack<State> _states;

        public State CurrentState
        {
            get => _states.Peek();
            private set
            {
                _ = _states.Pop();
                _states.Push(value);
            }
        }

        public StateStack()
        {
            _states = new Stack<State>((int)MAX_STATES);

            Save();
            Reset();
        }

        public void Save()
        {
            if (_states.Count >= MAX_STATES)
            {
                return;
            }
            else if (_states.Count > 0)
            {
                _states.Push(_states.Peek().Clone());
            }
            else
            {
                _states.Push(State.Default);
            }
        }

        public void Reset()
        {
            _states.Peek().Reset();
        }

        public void Restore()
        {
            if (_states.Count <= 1)
            {
                return;
            }
            _ = _states.Pop();
        }

        public void Clear()
        {
            _states.Clear();
        }

    }
}
