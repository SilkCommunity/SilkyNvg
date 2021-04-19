using FontStash.NET;
using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core;
using SilkyNvg.Core.Paths;
using SilkyNvg.Core.States;
using SilkyNvg.OpenGL;
using SilkyNvg.OpenGL.Textures;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Text
{
    internal class FontManager
    {

        public const int INVALID_FONT = Fontstash.INVALID;

        public const int INIT_FONTIMAGE_SIZE = 512;
        public const int MAX_FONTIMAGE_SIZE = 2048;

        private readonly IList<int> _fontImages = new List<int>();

        private readonly Fontstash _fons;
        private readonly GraphicsManager _graphicsManager;

        public Fontstash Fons => _fons;

        public FontManager(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;
            FonsParams prams = new()
            {
                width = INIT_FONTIMAGE_SIZE,
                height = INIT_FONTIMAGE_SIZE,
                flags = (byte)FonsFlags.ZeroTopleft,
                renderCreate = null,
                renderUpdate = null,
                renderDraw = null,
                renderDelete = null,
            };
            _fons = new Fontstash(prams);

            _fontImages.Add(graphicsManager.CreateTexture(TextureType.Alpha, FontManager.INIT_FONTIMAGE_SIZE, FontManager.INIT_FONTIMAGE_SIZE, 0, null));
            if (_fontImages[0] == 0)
                throw new Exception("Failed to create font image!");
        }

        private void FlushTextTexture()
        {
            if (_fons.ValidateTexture(out int[] dirty))
            {
                int fontImage = _fontImages[^1];
                if (fontImage != 0)
                {
                    byte[] data = _fons.GetTextureData(out int iw, out int ih);
                    int x = dirty[0];
                    int y = dirty[1];
                    int w = dirty[2] - x;
                    int h = dirty[3] - y;
                    _graphicsManager.UpdateTexture(fontImage, x, y, w, h, data);
                }
            }
        }

        private bool AllocTextAtlas()
        {
            try
            {
                FlushTextTexture();
                Vector2D<int> i = _graphicsManager.GetTextureSize(_fontImages[^1]);
                if (i.X > i.Y)
                {
                    i.Y *= 2;
                }
                else
                {
                    i.X *= 2;
                }
                if (i.X > MAX_FONTIMAGE_SIZE || i.Y > MAX_FONTIMAGE_SIZE)
                    i.X = i.Y = MAX_FONTIMAGE_SIZE;
                _fontImages.Add(_graphicsManager.CreateTexture(TextureType.Alpha, i.X, i.Y, 0, null));
                _fons.ResetAtlas(i.X, i.Y);
            } catch
            {
                // Catch any overflows, i.e. out of memory.
                return false;
            }
            return true;
        }

        private void RenderText(Vertex[] verts, State state, Style style)
        {
            Paint paint = state.Fill;

            paint.Image = _fontImages[^1];

            var innerCol = paint.InnerColour;
            var outerCol = paint.OuterColour;
            innerCol.A *= state.Alpha;
            outerCol.A *= state.Alpha;
            paint.InnerColour = innerCol;
            paint.OuterColour = outerCol;

            _graphicsManager.Triangles(paint, state.CompositeOperation, state.Scissor, verts, style.FringeWidth);
        }

        public float DrawText(float x, float y, string str, char end, State state, Style style, PathCache cache)
        {
            FonsTextIter prevIter = default;
            FonsQuad q = new();

            float scale = GetFontScale(state) * style.PixelRatio;
            float invscale = 1.0f / scale;
            bool isFlipped = Maths.IsTransformFlipped(state.Transform);

            if (state.FontId == Fontstash.INVALID)
                return x;

            _fons.SetSize(state.FontSize * scale);
            _fons.SetSpacing(state.LetterSpacing * scale);
            _fons.SetBlur(state.FontBlur * scale);
            _fons.SetAlign(state.TextAlign);
            _fons.SetFont(state.FontId);

            _fons.TextIterInit(out FonsTextIter iter, x * scale, y * scale, str, end, FonsGlyphBitmap.Requiered);
            prevIter = iter;
            var verts = new List<Vertex>();
            while (_fons.TextIterNext(ref iter, ref q))
            {
                if (iter.prevGlyphIndex == -1)
                {
                    if (verts.Count != 0)
                    {
                        RenderText(verts.ToArray(), state, style);
                        verts.Clear();
                    }
                    if (!AllocTextAtlas())
                        break;
                    iter = prevIter;
                    _ = _fons.TextIterNext(ref iter, ref q);
                    if (iter.prevGlyphIndex == -1)
                        break;
                }
                prevIter = iter;
                if (isFlipped)
                {
                    float tmp = q.y0;
                    q.y0 = q.y1;
                    q.y1 = tmp;
                    tmp = q.t0;
                    q.t0 = q.t1;
                    q.t1 = tmp;
                }

                Vector2D<float>[] c = new Vector2D<float>[4];
                c[0] = Maths.TransformPoint(new Vector2D<float>(q.x0 * invscale, q.y0 * invscale), state.Transform);
                c[1] = Maths.TransformPoint(new Vector2D<float>(q.x1 * invscale, q.y0 * invscale), state.Transform);
                c[2] = Maths.TransformPoint(new Vector2D<float>(q.x1 * invscale, q.y1 * invscale), state.Transform);
                c[3] = Maths.TransformPoint(new Vector2D<float>(q.x0 * invscale, q.y1 * invscale), state.Transform);

                VSet(c[0], q.s0, q.t0, verts, cache);
                VSet(c[2], q.s1, q.t1, verts, cache);
                VSet(c[1], q.s1, q.t0, verts, cache);
                VSet(c[0], q.s0, q.t0, verts, cache);
                VSet(c[3], q.s0, q.t1, verts, cache);
                VSet(c[2], q.s1, q.t1, verts, cache);
            }

            FlushTextTexture();
            RenderText(verts.ToArray(), state, style);

            return iter.nextx / scale;
        }

        private static float GetFontScale(State state)
        {
            return MathF.Min(Maths.Quantize(Maths.GetAverageScale(state.Transform), 0.01f), 4.0f);
        }

        private static void VSet(Vector2D<float> pos, float u, float v, List<Vertex> list, PathCache cache)
        {
            list.Add(new Vertex(pos.X, pos.Y, u, v));
            cache.Vertices.Add(list[^1]);
        }

    }
}
