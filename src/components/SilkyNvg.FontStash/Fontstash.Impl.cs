using System;

namespace FontStash.NET
{
    public partial class Fontstash
    {

        private const int APREC = 16;
        private const int ZPREC = 7;

        private void AddWhiteRect(int w, int h)
        {
            int gx = 0, gy = 0;
            if (!_atlas.AddRect(w, h, ref gx, ref gy))
                return;

            int index = gx + (gy * _params.width);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    _texData[index + x] = 0xff;
                }
                index += _params.width;
            }

            _dirtyRect[0] = Math.Min(_dirtyRect[0], gx);
            _dirtyRect[1] = Math.Min(_dirtyRect[1], gy);
            _dirtyRect[2] = Math.Max(_dirtyRect[2], gx + w);
            _dirtyRect[3] = Math.Max(_dirtyRect[3], gy + h);
        }

        private FonsState GetState()
        {
            if (_states[_nstates - 1] == null)
                _states[_nstates - 1] = new FonsState();
            return _states[_nstates - 1];
        }

        private int AllocFont()
        {
            if (_nfonts + 1 > _cfonts)
            {
                _cfonts = _cfonts == 0 ? 8 : _cfonts * 2;
                Array.Resize(ref _fonts, _cfonts);
            }
            FonsFont font = new();
            font.glyphs = new FonsGlyph[INIT_GLYPHS];
            font.cglyphs = INIT_GLYPHS;
            font.nglyphs = 0;

            _fonts[_nfonts++] = font;
            return _nfonts - 1;
        }

        private void BlurCols(int index, int w, int h, int dstStride, int alpha)
        {
            for (int y = 0; y < h; y++)
            {
                int z = 0;
                for (int x = 1; x < w; x++)
                {
                    z += (alpha * (((int)(_texData[index + x]) << ZPREC) - z)) >> APREC;
                    _texData[index + x] = (byte)(z >> ZPREC);
                }
                _texData[index + (w - 1)] = 0;
                z = 0;
                for (int x = w - 2; x >= 0; x--)
                {
                    z += (alpha * (((int)(_texData[index + x]) << ZPREC) - z)) >> APREC;
                    _texData[index + x] = (byte)(z >> ZPREC);
                }
                _texData[index + 0] = 0;
                index += dstStride;
            }
        }

        private void BlurRows(int index, int w, int h, int dstStride, int alpha)
        {
            for (int x = 0; x < w; x++)
            {
                int z = 0;
                for (int y = dstStride; y < h * dstStride; y += dstStride)
                {
                    z += (alpha * (((int)(_texData[index + y]) << ZPREC) - z)) >> APREC;
                    _texData[index + y] = (byte)(z >> ZPREC);
                }
                _texData[index + ((h - 1) * dstStride)] = 0;
                z = 0;
                for (int y = (h - 2) * dstStride; y >= 0; y -= dstStride)
                {
                    z += (alpha * (((int)(_texData[index + y]) << ZPREC) - z)) >> APREC;
                    _texData[index + y] = (byte)(z >> ZPREC);
                }
                _texData[index + 0] = 0;
                index++;
            }
        }

        private void Blur(int index, int w, int h, int dstStride, int blur)
        {
            if (blur < 1)
                return;

            float sigma = (float)blur * 0.57735f; // 0.57735 =~= 1 / Sqrt(3)
            int alpha = (int)((1 << APREC) * (1.0f - MathF.Exp(-2.3f / (sigma + 1.0f))));
            BlurRows(index, w, h, dstStride, alpha);
            BlurCols(index, w, h, dstStride, alpha);
            BlurRows(index, w, h, dstStride, alpha);
            BlurCols(index, w, h, dstStride, alpha);
        }

        private FonsGlyph GetGlyph(FonsFont font, uint codepoint, short isize, short iblur, FonsGlyphBitmap bitmapOption)
        {
            int gx = 0, gy = 0;
            FonsGlyph glyph = null;
            float size = isize / 10.0f;
            FonsFont renderFont = font;

            if (isize < 2)
                return null;
            if (iblur > 20)
                iblur = 20;
            int pad = iblur + 2;

            _nscratch = 0;

            uint h = Utils.HashInt(codepoint) & (HASH_LUT_SIZE - 1);
            int i = font.lut[h];
            while (i != -1)
            {
                if (font.glyphs[i].codepoint == codepoint && font.glyphs[i].size == isize && font.glyphs[i].blur == iblur)
                {
                    glyph = font.glyphs[i];
                    if (bitmapOption == FonsGlyphBitmap.Optional || (glyph.x0 >= 0 && glyph.y0 >= 0))
                    {
                        return glyph;
                    }
                    break;
                }
                i = font.glyphs[i].next;
            }

            int g = FonsTt.GetGlyphIndex(font.font, (int)codepoint);
            if (g == 0)
            {
                for (i = 0; i < font.nfallbacks; i++)
                {
                    FonsFont fallbackFont = _fonts[font.fallbacks[i]];
                    int fallbackIndex = FonsTt.GetGlyphIndex(fallbackFont.font, (int)codepoint);
                    if (fallbackIndex != 0)
                    {
                        g = fallbackIndex;
                        renderFont = fallbackFont;
                        break;
                    }
                }
            }

            float scale = FonsTt.GetPixelHeightScale(renderFont.font, size);
            FonsTt.BuildGlyphBitmap(renderFont.font, g, scale, out int advance, out int lsb, out int x0, out int y0, out int x1, out int y1);
            int gw = x1 - x0 + pad * 2;
            int gh = y1 - y0 + pad * 2;

            if (bitmapOption == FonsGlyphBitmap.Requiered)
            {
                bool added = _atlas.AddRect(gw, gh, ref gx, ref gy);
                if (!added)
                {
                    _handleError?.Invoke(FonsErrorCode.AtlasFull, 0);
                    added = _atlas.AddRect(gw, gh, ref gx, ref gy);
                }
                if (added == false)
                    return null;
            }
            else
            {
                gx = INVALID;
                gy = INVALID;
            }

            // Init glyph
            if (glyph == null)
            {
                glyph = font.AllocGlyph();
                glyph.codepoint = codepoint;
                glyph.size = isize;
                glyph.blur = iblur;
                glyph.next = 0;

                glyph.next = font.lut[h];
                font.lut[h] = font.nglyphs - 1;
            }
            glyph.index = g;
            glyph.x0 = (short)gx;
            glyph.y0 = (short)gy;
            glyph.x1 = (short)(glyph.x0 + gw);
            glyph.y1 = (short)(glyph.y0 + gh);
            glyph.xadv = (short)(scale * advance * 10.0f);
            glyph.xoff = (short)(x0 - pad);
            glyph.yoff = (short)(y0 - pad);

            if (bitmapOption == FonsGlyphBitmap.Optional)
            {
                return glyph;
            }

            // rasterize
            int index = (glyph.x0 + pad) + (glyph.y0 + pad) * _params.width;
            FonsTt.RenderGlyphBitmap(renderFont.font, _texData, index, gw - (pad * 2), gh - (pad * 2), _params.width, scale, scale, g);

            // Ensure border pixel
            index = glyph.x0 + (glyph.y0 * _params.width);
            for (int y = 0; y < gh; y++)
            {
                _texData[index + (y * _params.width)] = 0;
                _texData[index + (gw - 1 + y * _params.width)] = 0;
            }
            for (int x = 0; x < gw; x++)
            {
                _texData[index + x] = 0;
                _texData[index + ((gh - 1) * _params.width)] = 0;
            }

            if (iblur > 0)
            {
                _nscratch = 0;
                index = glyph.x0 + glyph.y0 * _params.width;
                Blur(index, gw, gh, _params.width, iblur);
            }

            _dirtyRect[0] = Math.Min(_dirtyRect[0], glyph.x0);
            _dirtyRect[1] = Math.Min(_dirtyRect[1], glyph.y0);
            _dirtyRect[2] = Math.Max(_dirtyRect[2], glyph.x1);
            _dirtyRect[3] = Math.Max(_dirtyRect[3], glyph.y1);

            return glyph;
        }

        private FonsQuad GetQuad(FonsFont font, int prevGlyphIndex, FonsGlyph glyph, float scale, float spacing, ref float x, ref float y)
        {
            FonsQuad q = new();

            if (prevGlyphIndex != INVALID)
            {
                float adv = FonsTt.GetGlyphKernAdvance(font.font, prevGlyphIndex, glyph.index) * scale;
                x += (int)(adv + spacing + 0.5f);
            }

            float xoff = (short)(glyph.xoff + 1);
            float yoff = (short)(glyph.yoff + 1);
            float x0 = (short)(glyph.x0 + 1);
            float y0 = (short)(glyph.y0 + 1);
            float x1 = (short)(glyph.x1 - 1);
            float y1 = (short)(glyph.y1 - 1);

            float rx, ry;
            if ((_params.flags & (byte)FonsFlags.ZeroTopleft) != 0)
            {
                rx = MathF.Floor(x + xoff);
                ry = MathF.Floor(y + yoff);

                q.x0 = rx;
                q.y0 = ry;
                q.x1 = rx + x1 - x0;
                q.y1 = ry + y1 - y0;

                q.s0 = x0 * _itw;
                q.t0 = y0 * _ith;
                q.s1 = x1 * _itw;
                q.t1 = y1 * _ith;
            }
            else
            {
                rx = MathF.Floor(x + xoff);
                ry = MathF.Floor(y - yoff);

                q.x0 = rx;
                q.y0 = ry;
                q.x1 = rx + x1 - x0;
                q.y1 = ry - y1 + y0;

                q.s0 = x0 * _itw;
                q.t0 = y0 * _ith;
                q.s1 = x1 * _itw;
                q.t1 = y1 * _ith;
            }

            x += (int)(glyph.xadv / 10.0f + 0.5f);
            return q;
        }

        private void Flush()
        {
            if (_dirtyRect[0] < _dirtyRect[2] && _dirtyRect[1] < _dirtyRect[3])
            {
                _params.renderUpdate?.Invoke(_dirtyRect, _texData);
                _dirtyRect[0] = _params.width;
                _dirtyRect[1] = _params.height;
                _dirtyRect[2] = 0;
                _dirtyRect[3] = 0;
            }

            if (_nverts > 0)
            {
                _params.renderDraw?.Invoke(_verts, _tcoords, _colours, _nverts);
                _nverts = 0;
            }
        }

        private void Vertex(float x, float y, float s, float t, uint c)
        {
            _verts[_nverts * 2 + 0] = x;
            _verts[_nverts * 2 + 1] = y;
            _tcoords[_nverts * 2 + 0] = s;
            _tcoords[_nverts * 2 + 1] = t;
            _colours[_nverts] = c;
            _nverts++;
        }

        private float GetVertAlign(FonsFont font, int align, short isize)
        {
            if ((_params.flags & (uint)FonsFlags.ZeroTopleft) != 0)
            {
                if ((align & (uint)FonsAlign.Top) != 0)
                {
                    return font.ascender * (float)isize / 10.0f;
                }
                else if ((align & (uint)FonsAlign.Middle) != 0)
                {
                    return (font.ascender + font.descender) / 2.0f * (float)isize / 10.0f;
                }
                else if ((align & (uint)FonsAlign.Baseline) != 0)
                {
                    return 0.0f;
                }
                else if ((align & (uint)FonsAlign.Bottom) != 0)
                {
                    return font.descender * (float)isize / 10.0f;
                }
            }
            else
            {
                if ((align & (uint)FonsAlign.Top) != 0)
                {
                    return -font.ascender * (float)isize / 10.0f;
                }
                else if ((align & (uint)FonsAlign.Middle) != 0)
                {
                    return -(font.ascender + font.descender) / 2.0f * (float)isize / 10.0f;
                }
                else if ((align & (uint)FonsAlign.Baseline) != 0)
                {
                    return 0.0f;
                }
                else if ((align & (uint)FonsAlign.Bottom) != 0)
                {
                    return -font.descender * (float)isize / 10.0f;
                }
            }

            return 0.0f;
        }

    }
}
