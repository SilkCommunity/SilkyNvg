using System.Collections.Generic;
using SilkyNvg.Rendering.Vulkan.Utils;

namespace SilkyNvg.Rendering.Vulkan.Calls
{
    public class CallQueue
    {
        NvgFrame _frame;
        public CallQueue(NvgFrame frame)
        {
            _frame = frame;
        }

        private readonly Queue<Call> _calls = new Queue<Call>();

        public bool HasCalls => _calls.Count > 0;
        public int CallCount => _calls.Count;

        public void Add(Call call)
        {
            _calls.Enqueue(call);
        }

        public List<DrawCall> CreateDrawCalls()
        {
            List<DrawCall> draws = new List<DrawCall>();
            foreach (Call call in _calls)
            {
                call.Run(_frame, draws);
            }

            return draws;
        }

        public void Clear()
        {
            _calls.Clear();
        }

    }
}