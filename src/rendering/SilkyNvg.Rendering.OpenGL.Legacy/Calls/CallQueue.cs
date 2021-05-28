using System.Collections.Generic;

namespace SilkyNvg.Rendering.OpenGL.Legacy.Calls
{
    internal class CallQueue
    {

        private readonly Queue<Call> _calls = new();

        public bool HasCalls => _calls.Count > 0;

        public CallQueue()
        {

        }

        public void AddCall(Call call)
        {
            _calls.Enqueue(call);
        }

        public void CallAll(LegacyOpenGLRenderer renderer)
        {
            while (_calls.Count > 0)
            {
                _calls.Dequeue().Run(renderer);
            }
        }

        public void Clear()
        {
            _calls.Clear();
        }

    }
}
