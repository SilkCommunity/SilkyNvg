using Silk.NET.Vulkan;
using System.Collections.Generic;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    internal class CallQueue
    {

        private readonly Queue<Call> _calls = new();

        public bool HasCalls => _calls.Count > 0;

        public void Add(Call call)
        {
            _calls.Enqueue(call);
        }

        public void Run(Frame frame, CommandBuffer commandBuffer)
        {
            foreach (Call call in _calls)
            {
                call.Run(frame, commandBuffer);
            }
        }

        public void Clear()
        {
            _calls.Clear();
        }

    }
}
