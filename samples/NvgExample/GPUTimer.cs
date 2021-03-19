using Silk.NET.OpenGL;

namespace NvgExample
{
    public class GPUTimer
    {

        private const int MAX_QUERY_COUNT = 5;

        private int _cur, _ret;
        private uint[] _queries;

        private readonly GL _gl;

        public GPUTimer(GL gl)
        {
            _gl = gl;
            _queries = new uint[MAX_QUERY_COUNT];
        }

        public void Start()
        {
            _gl.BeginQuery(QueryTarget.TimeElapsed, _queries[_cur % MAX_QUERY_COUNT]);
            _cur++;
        }

    }
}
