using SilkyNvg.OpenGL.Calls;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL.Instructions
{
    internal class CallQueue
    {

        private readonly Queue<Call> _callQueue;

        public int QueueLength => _callQueue.Count;

        public CallQueue()
        {
            _callQueue = new Queue<Call>();
        }

        private Call Next()
        {
            return _callQueue.Dequeue();
        }

        public void RunCalls(GLInterface glInterface, Silk.NET.OpenGL.GL gl)
        {
            while (QueueLength > 0)
            {
                Next().Run(glInterface, gl);
            }
        }

        public void Add(Call call)
        {
            _callQueue.Enqueue(call);
        }

        public void Clear()
        {
            _callQueue.Clear();
        }

    }
}