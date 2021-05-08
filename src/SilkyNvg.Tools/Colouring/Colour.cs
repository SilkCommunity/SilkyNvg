using System;
using System.Numerics;

namespace SilkyNvg
{
    /// <summary>
    /// <inheritdoc cref="Common.Docs.Colours"/>
    /// 
    /// Represents a colour stored as floats in RGBA format.
    /// </summary>
    public struct Colour
    {

        private Vector4 _rgba;

        /// <summary>
        /// X  is R
        /// Y is G
        /// Z is B
        /// W is A
        /// </summary>
        public Vector4 Rgba
        {
            get => _rgba;
            set => _rgba = value;
        }

        /// <summary>
        /// The red component.
        /// </summary>
        public float R
        {
            get => _rgba.X;
            set => _rgba.X = value;
        }

        /// <summary>
        /// The green component.
        /// </summary>
        public float G
        {
            get => _rgba.Y;
            set => _rgba.Y = value;
        }

        /// <summary>
        /// The blue component.
        /// </summary>
        public float B
        {
            get => _rgba.Z;
            set => _rgba.Z = value;
        }

        /// <summary>
        /// The alpha component.
        /// </summary>
        public float A
        {
            get => _rgba.W;
            set => _rgba.W = value;
        }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Create a new Colour from the rgba components
        /// between 0 and 1.
        /// </summary>
        /// <param name="rgba">The Rgba components</param>
        public Colour(Vector4 rgba)
        {
            _rgba = rgba;
        }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Alpha will be set to 255 (1.0F)
        /// </summary>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        public Colour(byte r, byte g, byte b) : this((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f) { }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Alpha will be set to 1.0F.
        /// </summary>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        public Colour(float r, float g, float b) : this(r, g, b, 1.0f) { }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// </summary>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        /// <param name="a">The alpha component</param>
        public Colour(byte r, byte g, byte b, byte a) : this((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f, (float)a / 255.0f) { }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        /// <param name="a">The alpha component.</param>
        /// <returns>A colour value from red, green, blue and alpha values.</returns>
        public Colour(float r, float g, float b, float a) : this(new Vector4(r, g, b, a)) { }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Linearly interpolates from c0 to c1 and returns the resulting colour.
        /// </summary>
        /// <param name="c0">The first colour</param>
        /// <param name="c1">The second colour</param>
        /// <param name="u">The gradient</param>
        public Colour(Colour c0, Colour c1, float u)
        {
            float r, g, b, a;

            u = MathF.Min(u, 1.0f);
            u = MathF.Max(u, 0.0f);
            float oneminus = 1.0f - u;
            r = c0.R * oneminus + c1.R * u;
            g = c0.G * oneminus + c1.G * u;
            b = c0.B * oneminus + c1.B * u;
            a = c0.A * oneminus + c1.A * u;

            _rgba = new Vector4(r, g, b, a);
        }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Set the transparency of a colour.
        /// </summary>
        /// <param name="colour">The origin colour</param>
        /// <param name="alpha">The new alpha component</param>
        public Colour(Colour colour, byte alpha) : this(colour.R, colour.G, colour.B, (float)alpha / 255.0f) { }

        /// <summary>
        /// <inheritdoc cref="Common.Docs.Colours"/>
        /// 
        /// Set the transparency of a colour.
        /// </summary>
        /// <param name="colour">The origin colour</param>
        /// <param name="alpha">The new alpha component</param>
        public Colour(Colour colour, float alpha) : this(colour.R, colour.G, colour.B, alpha) { }

        /// <summary>
        /// Premultiplies the colour by multiplying each value with the alpha value.
        /// </summary>
        /// <param name="colour">The colour to be premultiplied.</param>
        /// <returns>The premultiplied colour.</returns>
        public static Colour Premult(Colour colour)
        {
            return new Colour(colour.R * colour.A, colour.G * colour.A, colour.B * colour.A, colour.A);
        }

        /// <summary>
        /// Creates a new colour from a string.
        /// </summary>
        /// <param name="name">The colour string name.</param>
        /// <returns>A colour from the name in the string.</returns>
        public static Colour Get(string name)
        {
            Type colorType = typeof(System.Drawing.Color);
            foreach (var p in colorType.GetProperties())
            {
                if (p.Name.ToLower().Equals(name.ToLower()))
                {
                    System.Drawing.Color cssColor = (System.Drawing.Color)p.GetValue(null, null);

                    return new Colour(cssColor.R, cssColor.G, cssColor.B, cssColor.A);
                }
            }

            return new Colour();
        }

        internal static float Hue(float h, float m1, float m2)
        {
            if (h < 0.0f)
                h += 1.0f;
            if (h > 1.0f)
                h -= 1.0f;
            if (h < 1.0f / 6.0f)
                return m1 + (m2 - m1) * h * 6.0f;
            else if (h < 3.0f / 6.0f)
                return m2;
            else if (h < 4.0f / 6.0f)
                return m1 + (m2 - m1) * (2.0f / 3.0f - h) * 6.0f;
            return m1;
        }

    }
}