using Silk.NET.Maths;

namespace SilkyNvg
{
    public struct Colour
    {

        private Vector4D<float> _rgba;

        public Vector4D<float> Rgba
        {
            get => _rgba;
            set => _rgba = value;
        }

        public float R
        {
            get => _rgba.X;
            set => _rgba.X = value;
        }
        public float G
        {
            get => _rgba.Y;
            set => _rgba.Y = value;
        }
        public float B
        {
            get => _rgba.Z;
            set => _rgba.Z = value;
        }
        public float A
        {
            get => _rgba.W;
            set => _rgba.W = value;
        }

        /// <summary>
        /// Create a new colour using the following
        /// RGBA-Parameters, specified as parts of float.
        /// I.E. devide the 0-255 by 255.
        /// <seealso cref="Nvg.RGBAf(float, float, float, float)"/>
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        /// <returns>A colour using the specified rgba values.</returns>
        public Colour(float r, float g, float b, float a) : this(new Vector4D<float>(r, g, b, a)) { }

        public Colour(Vector4D<float> rgba)
        {
            _rgba = rgba;
        }

        public Colour(float r, float g, float b) : this(r, g, b, 1.0f) { }

        public Colour(Vector3D<float> rgb) : this(rgb.X, rgb.Y, rgb.Z) { }

        public Colour(byte r, byte g, byte b, byte a) : this(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f) { }

        public Colour(Vector4D<byte> rgba) : this(rgba.X, rgba.Y, rgba.Z, rgba.W) { }

        public Colour(byte r, byte g, byte b) : this(r, g, b, 255) { }

        public Colour(Vector3D<byte> rgb) : this(rgb.X, rgb.Y, rgb.Z) { }


    }
}
