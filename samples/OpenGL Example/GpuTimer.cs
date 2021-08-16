using Silk.NET.OpenGL;

namespace OpenGL_Example
{
    public class GpuTimer
    {

        private const uint GPU_QUERY_COUNT = 5;

        private readonly uint[] _queries = new uint[GPU_QUERY_COUNT];
        private readonly GL _gl;

        private uint _current;
        private uint _returns;

        public GpuTimer(GL gl)
        {
            _gl = gl;

            _gl.GenQueries(_queries);
            _current = 0;
            _returns = 0;
        }

        public void Start()
        {
            _gl.BeginQuery(QueryTarget.TimeElapsed, _queries[_current % GPU_QUERY_COUNT]);
            _current++;
        }

        public uint Stop(float[] times)
        {
            uint n = 0;

            _gl.EndQuery(QueryTarget.TimeElapsed);
            while (_returns <= _current)
            {
                _gl.GetQueryObject(_queries[_returns % GPU_QUERY_COUNT], QueryObjectParameterName.QueryResultAvailable, out int available);
                if (available != 0)
                {
                    _gl.GetQueryObject(_queries[_returns % GPU_QUERY_COUNT], QueryObjectParameterName.QueryResult, out uint timeElapsed);
                    _returns++;
                    if (n < times.Length)
                    {
                        times[n] = (float)((double)timeElapsed * 1e-9);
                        n++;
                    }
                }
            }

            return n;
        }

    }
}
