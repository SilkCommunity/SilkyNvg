using Silk.NET.Maths;

namespace SilkyNvg.Common
{
    public struct Vertex
    {

        public float X { get; private set; }
        public float Y { get; private set; }
        public float U { get; private set; }
        public float V { get; private set; }

        public Vector2D<float> Coordinates => new Vector2D<float>(X, Y);
        public Vector2D<float> TextureCoordinates => new Vector2D<float>(U, V);
        public Vector4D<float> Combined => new Vector4D<float>(X, Y, U, V);

        public Vertex(Vector2D<float> coordinates) : this(coordinates.X, coordinates.Y) { }
        public Vertex(Vector2D<float> coordinates, Vector2D<float> textureCoordinates) : this(coordinates.X, coordinates.Y, textureCoordinates.X, textureCoordinates.Y) { }
        public Vertex(Vector4D<float> combined) : this(combined.X, combined.Y, combined.Z, combined.W) { }
        public Vertex(float x, float y) : this(x, y, 0, 0) { }
        public Vertex(float x, float y, float u, float v)
        {
            X = x;
            Y = y;
            U = u;
            V = v;
        }

    }
}