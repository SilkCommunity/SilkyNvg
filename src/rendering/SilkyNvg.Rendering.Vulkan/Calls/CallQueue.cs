using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class CallQueue
    {

        private readonly Queue<Call> _calls = new();

        public bool HasCalls => _calls.Count > 0;

        public uint Count => (uint)_calls.Count;

        public CallQueue()
        {

        }

        public void Add(Call call)
        {
            _calls.Enqueue(call);
        }

        public void Run()
        {
            while (_calls.Count > 0)
            {
                _calls.Dequeue().Run();
            }
        }

    }
}
