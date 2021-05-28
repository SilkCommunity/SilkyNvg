using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace SilkyNvg
{

    [StructLayout(LayoutKind.Explicit)]
    public struct Vertex
    {

        [FieldOffset(0 * 4)] private float _x;
        [FieldOffset(1 * 4)] private float _y;
        [FieldOffset(2 * 4)] private float _u;
        [FieldOffset(3 * 4)] private float _v;

        public float X { get => _x; internal set => _x = value; }

        public float Y { get => _y; internal set => _y = value; }

        public float U { get => _u; internal set => _u = value; }

        public float V { get => _v; internal set => _v = value; }

        public Vector2D<float> Pos => new(_x, _y);

        public Vector2D<float> TexPos => new(_u, _v);

        public Vector4D<float> Data => new(_x, _y, _u, _v);

        public Vertex(float x, float y, float u, float v)
        {
            _x = x;
            _y = y;
            _u = u;
            _v = v;
        }

        public Vertex(Vector2D<float> pos, Vector2D<float> texPos) : this(pos.X, pos.Y, texPos.X, texPos.Y) { }

        public Vertex(Vector4D<float> data) : this(data.X, data.Y, data.Z, data.W) { }

        public Vertex(Vector2D<float> pos, float u, float v) : this(pos.X, pos.Y, u, v) { }

    }
}
