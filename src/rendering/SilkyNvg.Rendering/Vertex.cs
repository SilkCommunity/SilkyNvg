using System.Numerics;
using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering
{

    [StructLayout(LayoutKind.Explicit)]
    public struct Vertex
    {

        [FieldOffset(0 * 4)] private float _x;
        [FieldOffset(1 * 4)] private float _y;
        [FieldOffset(2 * 4)] private float _u;
        [FieldOffset(3 * 4)] private float _v;

        public float X { readonly get => _x; internal set => _x = value; }

        public float Y { readonly get => _y; internal set => _y = value; }

        public float U { readonly get => _u; internal set => _u = value; }

        public float V { readonly get => _v; internal set => _v = value; }

        public Vector2 Pos => new(_x, _y);

        public Vector2 TexPos => new(_u, _v);

        public Vector4 Data => new(_x, _y, _u, _v);

        public Vertex(float x, float y, float u, float v)
        {
            _x = x;
            _y = y;
            _u = u;
            _v = v;
        }

        public Vertex(Vector2 pos, Vector2 texPos) : this(pos.X, pos.Y, texPos.X, texPos.Y) { }

        public Vertex(Vector4 data) : this(data.X, data.Y, data.Z, data.W) { }

        public Vertex(Vector2 pos, float u, float v) : this(pos.X, pos.Y, u, v) { }

        public override string ToString()
        {
            string @base = base.ToString();
            return @base + " [" + _x + ", " + _y + ", " + _u + ", " + _v + "]";
        }

    }
}
