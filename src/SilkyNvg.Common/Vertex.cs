using System.Runtime.InteropServices;

namespace SilkyNvg.Common
{

    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {

        public float x, y;
        public float u, v;

    }
}
