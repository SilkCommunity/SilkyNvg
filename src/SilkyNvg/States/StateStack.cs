using System.Collections.Generic;

namespace SilkyNvg.States;

internal sealed class StateStack
{

    private readonly Stack<State> _stateStack = new();

    internal State CurrentState => _stateStack.Peek();

    internal StateStack()
    {
        _stateStack.Push(new State());
    }
    
    internal void Clear()
    {
        _stateStack.Clear();
        _stateStack.Push(new State());
    }

    internal void PushState()
    {
        _stateStack.Push(new State(CurrentState));
    }

    internal void PopState()
    {
        if (_stateStack.Count <= 1)
        {
            return;
        }
        _stateStack.Pop();
    }

}