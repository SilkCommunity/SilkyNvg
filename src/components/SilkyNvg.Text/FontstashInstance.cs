using FontStash.NET;
using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.States;
using SilkyNvg.Images;
using SilkyNvg.Rendering;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Text
{
    internal class FontstashInstance
    {

        private const int INIT_FONTIMAGE_SIZE = 512;
        private const uint MAX_FONTIMAGE_SIZE = 2048;
        private const int MAX_FONTIMAGES = 4;

        private static readonly IDictionary<Nvg, FontstashInstance> instances = new Dictionary<Nvg, FontstashInstance>();

        public static FontstashInstance GetInstance(Nvg nvg)
        {
            if (!instances.ContainsKey(nvg))
            {
                instances.Add(nvg, new FontstashInstance(nvg));
            }

            return instances[nvg];
        }

        private readonly Nvg _nvg;

        public Fontstash Fons { get; }

        public int[] FontImages { get; } = new int[MAX_FONTIMAGES];

        public int FontImageIdx { get; private set; }

        private FontstashInstance(Nvg nvg)
        {
            _nvg = nvg;
            _nvg.endFrameText += EndFrame;

            for (int i = 0; i < MAX_FONTIMAGES; i++)
            {
                FontImages[i] = 0;
            }

            FonsParams prams = default;
            prams.width = INIT_FONTIMAGE_SIZE;
            prams.height = INIT_FONTIMAGE_SIZE;
            prams.flags = (int)FonsFlags.ZeroTopleft;
            prams.renderCreate = null;
            prams.renderUpdate = null;
            prams.renderDraw = null;
            prams.renderDelete = null;
            Fons = new Fontstash(prams);

            FontImages[0] = _nvg.renderer.CreateTexture(Texture.Alpha, new Vector2D<uint>((uint)prams.width, (uint)prams.height), 0, null);
            FontImageIdx = 0;
        }

        public float GetFontScale()
        {
            return MathF.Min(Maths.Quantize(Maths.GetAverageScale(_nvg.stateStack.CurrentState.Transform), 0.01f), 4.0f);
        }

        public void FlushTextTexture()
        {
            if (Fons.ValidateTexture(out int[] dirty))
            {
                int fontImage = FontImages[FontImageIdx];

                if (fontImage != 0)
                {
                    byte[] data = Fons.GetTextureData(out _, out _);
                    uint x = (uint)dirty[0];
                    uint y = (uint)dirty[1];
                    uint w = (uint)(dirty[2] - dirty[0]);
                    uint h = (uint)(dirty[3] - dirty[1]);
                    _nvg.renderer.UpdateTexture(fontImage, new Vector4D<uint>(x, y, w, h), data);
                }
            }
        }

        public bool AllocTextAtlas()
        {
            FlushTextTexture();
            if (FontImageIdx >= MAX_FONTIMAGES - 1)
            {
                return false;
            }

            Vector2D<uint> size;
            if (FontImages[FontImageIdx + 1] != 0)
            {
                _nvg.ImageSize(FontImages[FontImageIdx + 1], out size);
            }
            else
            {
                _nvg.ImageSize(FontImages[FontImageIdx], out size);

                if (size.X > size.Y)
                {
                    size.Y *= 2;
                }
                else
                {
                    size.X *= 2;
                }

                if (size.X > MAX_FONTIMAGE_SIZE || size.Y > MAX_FONTIMAGE_SIZE)
                {
                    size = new Vector2D<uint>(MAX_FONTIMAGE_SIZE);
                }

                FontImages[FontImageIdx + 1] = _nvg.renderer.CreateTexture(Texture.Alpha, size, 0, null);
            }
            FontImageIdx++;
            Fons.ResetAtlas((int)size.X, (int)size.Y);
            return true;
        }

        public void RenderText(Vertex[] vertices)
        {
            State state = _nvg.stateStack.CurrentState;
            Paint paint = Paint.ForText(FontImages[FontImageIdx], state.Fill);

            paint.PremultiplyAlpha(state.Alpha);

            _nvg.renderer.Triangles(paint, state.CompositeOperation, state.Scissor, vertices, _nvg.pixelRatio.FringeWidth);

            _nvg.FrameMeta.Update((uint)vertices.Length / 3, 0, 0, 1);
        }

        private void EndFrame()
        {
            if (FontImageIdx != 0)
            {
                int fontImage = FontImages[FontImageIdx];

                if (fontImage == 0)
                {
                    return;
                }

                _nvg.ImageSize(fontImage, out Vector2D<uint> iSize);

                uint i, j;
                for (i = j = 0; i < FontImageIdx; i++)
                {
                    Vector2D<uint> nSize = default;

                    if (FontImages[i] != 0)
                    {
                        _nvg.ImageSize(FontImages[i], out nSize);
                    }

                    if (nSize.X < iSize.X || nSize.Y < iSize.Y)
                    {
                        _nvg.DeleteImage(FontImages[FontImageIdx]);
                    }
                    else
                    {
                        FontImages[j++] = FontImages[i];
                    }
                }

                FontImages[j++] = FontImages[0];
                FontImages[0] = fontImage;
                FontImageIdx = 0;

                for (i = 0; i < MAX_FONTIMAGE_SIZE; i++)
                {
                    FontImages[i] = 0;
                }
            }
        }

    }
}
