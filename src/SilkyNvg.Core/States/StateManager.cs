using System.Collections.Generic;

namespace SilkyNvg.Core.States
{
    public sealed class StateManager
    {

        public const int MAX_STATES = 32;

        private readonly Stack<State> _stateStack;

        public StateManager()
        {
            _stateStack = new Stack<State>();
            _stateStack.Clear();
        }

        public void Save()
        {

        }

        public void Restore()
        {

        }

        public void Reset()
        {

        }

    }
}
