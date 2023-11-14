using Silk.NET.Maths;
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
        [FieldOffset(4 * 4)] private float _s;
        [FieldOffset(5 * 4)] private float _t;

        public float X { get => _x; internal set => _x = value; }

        public float Y { get => _y; internal set => _y = value; }

        public float U { get => _u; internal set => _u = value; }

        public float V { get => _v; internal set => _v = value; }
        
        public float S { get => _s; internal set => _s = value; }

        public float T { get => _t; internal set => _t = value; }
        
        public Vector2D<float> Pos => new(_x, _y);

        public Vector4D<float> TexPos => new(_u, _v, _s, _t);
        
        public Vertex(float x, float y, float u, float v, float s, float t)
        {
            _x = x;
            _y = y;
            _u = u;
            _v = v;
            _s = s;
            _t = t;
        }
        
        public Vertex(float x, float y, float u, float v) : this(x, y, u, v, 0f, 0f) { }
        
        public Vertex(Vector2D<float> pos, Vector2D<float> texPos) : this(pos.X, pos.Y, texPos.X, texPos.Y) { }

        public Vertex(Vector4D<float> data) : this(data.X, data.Y, data.Z, data.W) { }

        public Vertex(Vector2D<float> pos, float u, float v) : this(pos.X, pos.Y, u, v) { }
        public Vertex(Vector2D<float> pos, float u, float v, float s, float t) : this(pos.X, pos.Y, u, v, s, t) { }

        public override string ToString()
        {
            string @base = base.ToString();
            return @base + " [" + _x + ", " + _y + ", " + _u + ", " + _v + "]";
        }

    }
}
