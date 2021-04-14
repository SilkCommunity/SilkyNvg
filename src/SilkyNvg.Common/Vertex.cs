using Silk.NET.Maths;
using System.Runtime.InteropServices;

namespace SilkyNvg.Common
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {

        private readonly float _x;
        private readonly float _y;
        private readonly float _u;
        private readonly float _v;

        public float X => _x;
        public float Y => _y;
        public float U => _u;
        public float V => _v;

        public Vector2D<float> Coordinates => new Vector2D<float>(X, Y);
        public Vector2D<float> TextureCoordinates => new Vector2D<float>(U, V);
        public Vector4D<float> Combined => new Vector4D<float>(X, Y, U, V);

        public Vertex(Vector2D<float> coordinates) : this(coordinates.X, coordinates.Y) { }
        public Vertex(Vector2D<float> coordinates, Vector2D<float> textureCoordinates) : this(coordinates.X, coordinates.Y, textureCoordinates.X, textureCoordinates.Y) { }
        public Vertex(Vector4D<float> combined) : this(combined.X, combined.Y, combined.Z, combined.W) { }
        public Vertex(float x, float y) : this(x, y, 0, 0) { }
        public Vertex(float x, float y, float u, float v)
        {
            _x = x;
            _y = y;
            _u = u;
            _v = v;
        }

    }
}