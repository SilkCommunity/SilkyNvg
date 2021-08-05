using SilkyNvg.Common;
using System.Runtime.InteropServices;

namespace SilkyNvg
{

    /// <summary>
    /// SilkyNvg colours are stored as floats in the ARGB format.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Colour
    {

        #region Red Colour Constants
        public static Colour IndianRed { get; } = new(205, 92, 92);

        public static Colour LightCoral { get; } = new(240, 128, 128);

        public static Colour Salmon { get; } = new(250, 128, 114);

        public static Colour DarkSalmon { get; } = new(233, 150, 122);

        public static Colour LightSalmon { get; } = new(255, 160, 122);

        public static Colour Crimson { get; } = new(220, 20, 60);

        public static Colour Red { get; } = new(255, 0, 0);

        public static Colour FireBrick { get; } = new(178, 34, 34);

        public static Colour DarkRed { get; } = new(139, 0, 0);
        #endregion

        #region Pink Colour Constants
        public static Colour Pink { get; } = new(255, 192, 203);

        public static Colour LightPink { get; } = new(255, 182, 193);

        public static Colour HotPink { get; } = new(255, 105, 180);

        public static Colour DeepPink { get; } = new(255, 20, 147);

        public static Colour MediumVioletRed { get; } = new(199, 21, 133);

        public static Colour PaleVioletRed { get; } = new(219, 112, 147);
        #endregion

        #region Orange Colour Constants
        public static Colour Coral { get; } = new Colour(255, 127, 80);

        public static Colour Tomato { get; } = new Colour(255, 99, 71);

        public static Colour OrangeRed { get; } = new Colour(255, 69, 0);

        public static Colour DarkOrange { get; } = new Colour(255, 140, 0);

        public static Colour Orange { get; } = new Colour(255, 165, 0);
        #endregion

        #region Yellow Colour Constants
        public static Colour Gold { get; } = new Colour(255, 215, 0);

        public static Colour Yellow { get; } = new Colour(255, 255, 0);

        public static Colour LightYellow { get; } = new Colour(255, 255, 224);

        public static Colour LemonChiffon { get; } = new Colour(255, 250, 205);

        public static Colour LightGoldenrodYellow { get; } = new Colour(250, 250, 210);

        public static Colour PapayaWhip { get; } = new Colour(255, 239, 213);

        public static Colour Moccasin { get; } = new Colour(255, 228, 181);

        public static Colour PeachPuff { get; } = new Colour(255, 218, 185);

        public static Colour PaleGoldenrod { get; } = new Colour(238, 232, 170);

        public static Colour Khaki { get; } = new Colour(240, 230, 140);

        public static Colour DarkKhaki { get; } = new Colour(189, 183, 107);
        #endregion

        #region Purple Colour Constants
        public static Colour Lavender { get; } = new Colour(230, 230, 250);

        public static Colour Thistle { get; } = new Colour(216, 191, 216);

        public static Colour Plum { get; } = new Colour(221, 160, 221);

        public static Colour Violet { get; } = new Colour(238, 130, 238);

        public static Colour Orchid { get; } = new Colour(218, 112, 214);

        public static Colour Fuchsia { get; } = new Colour(255, 0, 255);

        public static Colour Magenta { get; } = new Colour(255, 0, 255);

        public static Colour MediumOrchid { get; } = new Colour(186, 85, 211);

        public static Colour MediumPurple { get; } = new Colour(147, 112, 219);

        public static Colour BlueViolet { get; } = new Colour(138, 43, 226);

        public static Colour DarkViolet { get; } = new Colour(148, 0, 211);

        public static Colour DarkOrchid { get; } = new Colour(153, 50, 204);

        public static Colour DarkMagenta { get; } = new Colour(139, 0, 139);

        public static Colour Purple { get; } = new Colour(128, 0, 128);

        public static Colour RebeccaPurple { get; } = new Colour(102, 51, 153);

        public static Colour Indigo { get; } = new Colour(75, 0, 130);

        public static Colour MediumSlateBlue { get; } = new Colour(123, 104, 238);

        public static Colour SlateBlue { get; } = new Colour(107, 90, 205);

        public static Colour DarkSlateBlue { get; } = new Colour(72, 61, 139);
        #endregion

        #region White Colour Constants
        public static Colour White { get; } = new(255, 255, 255);

        public static Colour Snow { get; } = new(255, 250, 250);

        public static Colour Honeydew { get; } = new(240, 255, 240);

        public static Colour MintCream { get; } = new(245, 255, 250);

        public static Colour Azure { get; } = new(240, 255, 255);

        public static Colour AliceBlue { get; } = new(240, 248, 255);

        public static Colour GhostWhite { get; } = new(248, 248, 255);

        public static Colour WhiteSmoke { get; } = new(245, 245, 245);

        public static Colour Seashell { get; } = new(255, 245, 238);

        public static Colour Beige { get; } = new(245, 245, 220);

        public static Colour OldLace { get; } = new(253, 245, 230);

        public static Colour FloralWhite { get; } = new(255, 250, 240);

        public static Colour Ivory { get; } = new(255, 255, 240);

        public static Colour AntiqueWhite { get; } = new(250, 235, 215);

        public static Colour Linen { get; } = new(250, 240, 230);

        public static Colour LavenderBlush { get; } = new(255, 240, 245);

        public static Colour MistyRose { get; } = new(255, 228, 225);
        #endregion

        #region Gray Colour Constants
        public static Colour Gainsboro { get; } = new(220, 220, 200);

        public static Colour LightGray { get; } = new(211, 211, 211);

        public static Colour Silver { get; } = new(192, 192, 192);

        public static Colour DarkGray { get; } = new(169, 169, 169);

        public static Colour Gray { get; } = new(128, 128, 128);

        public static Colour DimGray { get; } = new(105, 105, 105);

        public static Colour LightSlateGray { get; } = new(119, 136, 152);

        public static Colour SlateGray { get; } = new(112, 136, 153);

        public static Colour DarkSlateGray { get; } = new(47, 79, 79);

        public static Colour Black { get; } = new(0, 0, 0);
        #endregion

        [FieldOffset(0 * sizeof(float))] private readonly float _r;
        [FieldOffset(1 * sizeof(float))] private readonly float _g;
        [FieldOffset(2 * sizeof(float))] private readonly float _b;
        [FieldOffset(3 * sizeof(float))] private readonly float _a;

        public float R => _r;

        public float G => _g;

        public float B => _b;

        public float A => _a;

        #region Constructors
        public Colour(byte r, byte g, byte b) : this(r, g, b, 255) { }

        public Colour(float r, float g, float b) : this(r, g, b, 1.0f) { }

        public Colour(byte r, byte g, byte b, byte a)
        {
            _r = (float)r / 255.0f;
            _g = (float)g / 255.0f;
            _b = (float)b / 255.0f;
            _a = (float)a / 255.0f;
        }

        public Colour(float r, float g, float b, float a)
        {
            _r = r;
            _g = g;
            _b = b;
            _a = a;
        }

        public Colour(Colour c, byte a)
        {
            _r = c.R;
            _g = c.G;
            _b = c.B;
            _a = (float)a / 255.0f;
        }

        public Colour(Colour c, float a)
        {
            _r = c.R;
            _g = c.G;
            _b = c.B;
            _a = a;
        }

        public Colour(Colour c0, Colour c1, float u)
        {
            u = Maths.Clamp(u, 0.0f, 1.0f);
            float oneminusu = 1.0f - u;
            _r = c0.R * oneminusu + c1.R * u;
            _g = c0.G * oneminusu + c1.G * u;
            _b = c0.B * oneminusu + c1.B * u;
            _a = c0.A * oneminusu + c1.A * u;
        }

        public Colour(float h, float s, float l, byte a)
        {
            h = Maths.Mod(h, 1.0f);

            if (h < 0.0f)
                h += 1.0f;

            s = Maths.Clamp(s, 0.0f, 1.0f);
            l = Maths.Clamp(l, 0.0f, 1.0f);
            float m2 = l <= 0.5f ? (l * (l + s)) : (l + s - l * s);
            float m1 = 2 * l - m2;
            _r = Maths.Clamp(Hue(h + 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            _g = Maths.Clamp(Hue(h, m1, m2), 0.0f, 1.0f);
            _b = Maths.Clamp(Hue(h - 1.0f / 3.0f, m1, m2), 0.0f, 1.0f);
            _a = a / 255.0f;
        }
        #endregion

        public float this[int index]
        {
            get => index switch
            {
                0 => _r,
                1 => _g,
                2 => _b,
                3 => _a,
                _ => throw new System.IndexOutOfRangeException("Maximum index for SilkyNvg.Colour is 4!")
            };
        }

        public Colour Premult()
        {
            float r = _r * _a;
            float g = _g * _a;
            float b = _b * _a;
            return new Colour(r, g, b, _a);
        }

        internal Colour Premultiply(float alpha)
        {
            float a = _a * alpha;
            return new Colour(_r, _g, _b, a);
        }

        private static float Hue(float h, float m1, float m2)
        {
            if (h < 0.0f)
                h += 1.0f;
            if (h > 1.0f)
                h -= 1.0f;

            if (h < 1.0f / 6.0f)
            {
                return m1 + (m2 - m1) * h * 6.0f;
            }
            else if (h < 3.0f / 6.0f)
            {
                return m2;
            }
            else if (h < 4.0f / 6.0f)
            {
                return m1 + (m2 - m1) * (2.0f / 3.0f - h) * 6.0f;
            }

            return m1;
        }

    }
}
