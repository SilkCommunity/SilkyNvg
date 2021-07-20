using FontStash.NET;
using Silk.NET.Maths;
using SilkyNvg.Common;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using SilkyNvg.Transforms;
using System;
using System.Collections.Generic;

namespace SilkyNvg.Text
{
    public static class NvgText
    {

        public static int CreateFont(this Nvg nvg, string name, string fileName)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFont(name, fileName, 0);
        }

        public static int CreateFontAtIndex(this Nvg nvg, string name, string fileName, int fontIndex)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFont(name, fileName, fontIndex);
        }

        public static int CreateFontMem(this Nvg nvg, string name, byte[] data, int freeData)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFontMem(name, data, freeData);
        }

        public static int CreateFontMemAtIndex(this Nvg nvg, string name, byte[] data, int freeData, int fontIndex)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFontMem(name, data, freeData, fontIndex);
        }

        public static int FindFont(this Nvg nvg, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.GetFontByName(name);
        }

        public static bool AddFallbackFontId(this Nvg nvg, int baseFont, int fallbackFont)
        {
            if (baseFont == -1 || fallbackFont == -1)
            {
                return false;
            }

            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFallbackFont(baseFont, fallbackFont);
        }

        public static bool AddFallbackFont(this Nvg nvg, string baseFont, string fallbackFont)
        {
            return AddFallbackFontId(nvg, FindFont(nvg, baseFont), FindFont(nvg, fallbackFont));
        }

        public static void ResetFallbackFontsId(this Nvg nvg, int baseFont)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            fons.ResetFallbackFont(baseFont);
        }

        public static void ResetFallbackFonts(this Nvg nvg, string baseFont)
        {
            ResetFallbackFontsId(nvg, FindFont(nvg, baseFont));
        }

        public static void FontSize(this Nvg nvg, float size)
        {
            nvg.stateStack.CurrentState.FontSize = size;
        }

        public static void FontBlur(this Nvg nvg, float blur)
        {
            nvg.stateStack.CurrentState.FontBlur = blur;
        }

        public static void TextLetterSpacing(this Nvg nvg, float spacing)
        {
            nvg.stateStack.CurrentState.LetterSpacing = spacing;
        }

        public static void TextLineHeight(this Nvg nvg, float lineHeight)
        {
            nvg.stateStack.CurrentState.LineHeight = lineHeight;
        }

        public static void TextAlign(this Nvg nvg, Align align)
        {
            nvg.stateStack.CurrentState.TextAlign = align;
        }

        public static void FontFaceId(this Nvg nvg, int font)
        {
            nvg.stateStack.CurrentState.FontId = font;
        }

        public static void FontFace(this Nvg nvg, string font)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            nvg.stateStack.CurrentState.FontId = fons.GetFontByName(font);
        }

        public static float Text(this Nvg nvg, Vector2D<float> pos, string @string, string end)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;

            FonsQuad q = new();

            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            bool isFlipped = Maths.IsTransformFlipped(state.Transform);

            if (state.FontId == Fontstash.INVALID)
            {
                return pos.X;
            }

            fons.SetSize(state.FontSize * scale);
            fons.SetSpacing(state.LetterSpacing * scale);
            fons.SetBlur(state.FontBlur * scale);
            fons.SetAlign((int)state.TextAlign);
            fons.SetFont(state.FontId);

            List<Vertex> vertices = new();
            fons.TextIterInit(out FonsTextIter iter, pos.X * scale, pos.Y * scale, @string, end, FonsGlyphBitmap.Requiered);
            FonsTextIter prevIter = iter;
            while (fons.TextIterNext(ref iter, ref q))
            {
                Vector2D<float>[] c = new Vector2D<float>[4];

                if (iter.prevGlyphIndex == -1)
                {
                    if (vertices.Count != 0)
                    {
                        nvg.fontManager.RenderText(vertices);
                        vertices.Clear();
                    }
                    if (!nvg.fontManager.AllocTextAtlas()) // no memory
                    {
                        break;
                    }
                    iter = prevIter;
                    _ = fons.TextIterNext(ref iter, ref q);
                    if (iter.prevGlyphIndex == -1)
                    {
                        break;
                    }
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

                c[0] = nvg.TransformPoint(state.Transform, q.x0 * invscale, q.y0 * invscale);
                c[1] = nvg.TransformPoint(state.Transform, q.x1 * invscale, q.y0 * invscale);
                c[2] = nvg.TransformPoint(state.Transform, q.x1 * invscale, q.y1 * invscale);
                c[3] = nvg.TransformPoint(state.Transform, q.x0 * invscale, q.y1 * invscale);

                vertices.Add(new Vertex(c[0], q.s0, q.t0));
                vertices.Add(new Vertex(c[2], q.s1, q.t1));
                vertices.Add(new Vertex(c[1], q.s1, q.t0));
                vertices.Add(new Vertex(c[0], q.s0, q.t0));
                vertices.Add(new Vertex(c[3], q.s0, q.t1));
                vertices.Add(new Vertex(c[2], q.s1, q.t1));
            }

            nvg.fontManager.FlushTextTexture();
            nvg.fontManager.RenderText(vertices.ToArray());

            return iter.nextx / scale;
        }

        public static float Text(this Nvg nvg, Vector2D<float> position, string @string)
            => Text(nvg, position, @string, null);

        public static float Text(this Nvg nvg, float x, float y, string @string, string end)
            => Text(nvg, new Vector2D<float>(x, y), @string, end);

        public static float Text(this Nvg nvg, float x, float y, string @string)
            => Text(nvg, new Vector2D<float>(x, y), @string, null);

        public static void TextBox(this Nvg nvg, Vector2D<float> position, float breakRowWidth, string @string, string end)
        {
            State state = nvg.stateStack.CurrentState;
            int rowCount;
            Align oldAlign = state.TextAlign;
            Align hAlign = state.TextAlign & (Align.Left | Align.Centre | Align.Right);
            Align vAlign = state.TextAlign & (Align.Top | Align.Middle | Align.Bottom | Align.Baseline);

            if (state.FontId == Fontstash.INVALID)
            {
                return;
            }

            TextMetrics(nvg, out _, out _, out float lineh);

            state.TextAlign = Align.Left | vAlign;

            while ((rowCount = TextBreakLines(nvg, @string, end, breakRowWidth, out TextRow[] rows, 2)) != 0)
            {
                for (int i = 0; i < rowCount; i++)
                {
                    if (hAlign.HasFlag(Align.Left))
                    {
                        _ = Text(nvg, position, rows[i].Start, rows[i].End.Length > 0 ? rows[i].End : null) ;
                    }
                    else if (hAlign.HasFlag(Align.Centre))
                    {
                        _ = Text(nvg, position.X + breakRowWidth * 0.5f, position.Y - rows[i].Width * 0.5f, rows[i].Start, rows[i].End.Length > 0 ? rows[i].End : null);
                    }
                    else if (hAlign.HasFlag(Align.Right))
                    {
                        _ = Text(nvg, position.X + breakRowWidth - rows[i].Width, position.Y, rows[i].Start, rows[i].End.Length > 0 ? rows[i].End : null);
                    }
                    position.Y += lineh * state.LineHeight;
                }
                @string = rows[rowCount - 1].Next;
            }

            state.TextAlign = oldAlign;
        }

        public static void TextBox(this Nvg nvg, Vector2D<float> position, float breakRowWidth, string @string)
            => TextBox(nvg, position, breakRowWidth, @string, null);

        public static void TextBox(this Nvg nvg, float x, float y, float breakRowWidth, string @string, string end)
            => TextBox(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, end);

        public static void TextBox(this Nvg nvg, float x, float y, float breakRowWidth, string @string)
            => TextBox(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, null);

        public static float TextBounds(this Nvg nvg, Vector2D<float> position, string @string, string end, out Rectangle<float> bounds)
        {
            bounds = default;

            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;

            if (state.FontId == Fontstash.INVALID)
            {
                return 0;
            }

            fons.SetSize(state.FontSize * scale);
            fons.SetSpacing(state.LetterSpacing * scale);
            fons.SetBlur(state.FontBlur * scale);
            fons.SetAlign((int)state.TextAlign);
            fons.SetFont(state.FontId);

            float width = fons.TextBounds(position.X * scale, position.Y * scale, @string, end, out float[] bs);
            if (bs != null)
            {
                fons.LineBounds(position.Y * scale, out bs[1], out bs[3]);
                bounds = new Rectangle<float>()
                {
                    Origin = new Vector2D<float>(bs[0] * invscale, bs[1] * invscale),
                    Size = new Vector2D<float>((bs[2] - bs[0]) * invscale, (bs[3] - bs[1]) * invscale)
                };
            }

            return width * invscale;
        }

        public static float TextBounds(this Nvg nvg, Vector2D<float> position, string @string, out Rectangle<float> bounds)
            => TextBounds(nvg, position, @string, null, out bounds);

        public static float TextBounds(this Nvg nvg, float x, float y, string @string, string end, out Rectangle<float> bounds)
            => TextBounds(nvg, new Vector2D<float>(x, y), @string, end, out bounds);

        public static float TextBounds(this Nvg nvg, float x, float y, string @string, out Rectangle<float> bounds)
            => TextBounds(nvg, new Vector2D<float>(x, y), @string, null, out bounds);

        public static void TextBoxBounds(this Nvg nvg, Vector2D<float> position, float breakRowWidth, string @string, string end, out Rectangle<float> bounds)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            int nrows = 0;
            Align oldAlign = state.TextAlign;
            Align hAlign = state.TextAlign & (Align.Left | Align.Centre | Align.Right);
            Align vAlign = state.TextAlign & (Align.Top | Align.Middle | Align.Bottom | Align.Baseline);

            if (state.FontId == Fontstash.INVALID)
            {
                bounds = default;
                return;
            }

            nvg.TextMetrics(out _, out _, out float lineh);

            state.TextAlign = Align.Left | vAlign;

            float minX = position.X, maxX = position.X;
            float minY = position.Y, maxY = position.Y;

            fons.SetSize(state.FontSize * scale);
            fons.SetSpacing(state.LetterSpacing * scale);
            fons.SetBlur(state.FontBlur * scale);
            fons.SetAlign((int)state.TextAlign);
            fons.SetFont(state.FontId);
            fons.LineBounds(0, out float rMinY, out float rMaxY);
            rMinY *= invscale;
            rMaxY *= invscale;

            while ((nrows = TextBreakLines(nvg, @string, end, breakRowWidth, out TextRow[] rows, 2)) != 0)
            {
                for (uint i = 0; i < nrows; i++)
                {
                    float rMinX, rMaxX;
                    float dx = 0.0f;
                    if (hAlign.HasFlag(Align.Left))
                    {
                        dx = 0.0f;
                    }
                    else if (hAlign.HasFlag(Align.Centre))
                    {
                        dx = breakRowWidth * 0.5f - rows[i].Width * 0.5f;
                    }
                    else if (hAlign.HasFlag(Align.Right))
                    {
                        dx = breakRowWidth - rows[i].Width;
                    }
                    rMinX = position.X + rows[i].MinX + dx;
                    rMaxX = position.X + rows[i].MaxX + dx;
                    minX = MathF.Min(minX, rMinX);
                    maxX = MathF.Max(maxX, rMaxX);

                    minY = MathF.Min(minY, position.Y + rMinY);
                    maxY = MathF.Max(maxY, position.Y + rMaxY);

                    position.Y += lineh * state.LineHeight;
                }
                @string = rows[nrows - 1].Next;
            }

            state.TextAlign = oldAlign;

            bounds = new Rectangle<float>(new Vector2D<float>(minX, minY), new Vector2D<float>(maxX, maxY) - new Vector2D<float>(minX, minY));
        }

        public static void TextBoxBounds(this Nvg nvg, Vector2D<float> position, float breakRowWidth, string @string, out Rectangle<float> bounds)
            => TextBoxBounds(nvg, position, breakRowWidth, @string, null, out bounds);

        public static void TextBoxBounds(this Nvg nvg, float x, float y, float breakRowWidth, string @string, string end, out Rectangle<float> bounds)
            => TextBoxBounds(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, end, out bounds);

        public static void TextBoxBounds(this Nvg nvg, float x, float y, float breakRowWidth, string @string, out Rectangle<float> bounds)
            => TextBoxBounds(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, null, out bounds);

        public static int TextGlyphPositions(this Nvg nvg, Vector2D<float> position, string @string, string end, out GlyphPosition[] positions, int maxRows)
        {
            positions = null;

            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            List<GlyphPosition> ps = new(maxRows);
            FonsQuad q = new();

            if (state.FontId == Fontstash.INVALID)
            {
                return 0;
            }

            if (@string == end)
            {
                return 0;
            }

            fons.SetSize(state.FontSize * scale);
            fons.SetSpacing(state.LetterSpacing * scale);
            fons.SetBlur(state.FontBlur * scale);
            fons.SetAlign((int)state.TextAlign);
            fons.SetFont(state.FontId);

            fons.TextIterInit(out FonsTextIter iter, position.X * scale, position.Y * scale, @string, end, FonsGlyphBitmap.Optional);
            FonsTextIter prevIter = iter;
            while (fons.TextIterNext(ref iter, ref q))
            {
                if (iter.prevGlyphIndex < 0 && nvg.fontManager.AllocTextAtlas())
                {
                    iter = prevIter;
                    fons.TextIterNext(ref iter, ref q);
                }
                prevIter = iter;
                ps.Add(new GlyphPosition(iter.str, iter.x * invscale, MathF.Min(iter.x, q.x0) * invscale, MathF.Max(iter.nextx, q.x1) * invscale));
                if (ps.Count >= maxRows)
                {
                    positions = ps.ToArray();
                    return ps.Count;
                }
            }

            positions = ps.ToArray();
            return ps.Count;
        }

        public static int TextGlyphPositions(this Nvg nvg, float x, float y, string @string, string end, out GlyphPosition[] positions, int maxRows)
            => TextGlyphPositions(nvg, new Vector2D<float>(x, y), @string, end, out positions, maxRows);

        public static void TextMetrics(this Nvg nvg, out float ascender, out float descender, out float lineh)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;

            if (state.FontId == Fontstash.INVALID)
            {
                ascender = descender = lineh = -1.0f;
                return;
            }

            fons.SetSize(state.FontSize * scale);
            fons.SetSpacing(state.LetterSpacing * scale);
            fons.SetBlur(state.FontBlur * scale);
            fons.SetAlign((int)state.TextAlign);
            fons.SetFont(state.FontId);

            fons.VertMetrics(out ascender, out descender, out lineh);
            ascender *= invscale;
            descender *= invscale;
            lineh *= invscale;
        }

        public static int TextBreakLines(this Nvg nvg, string @string, string end, float breakRowWidth, out TextRow[] rows, int maxRows)
        {
            rows = null;

            Fontstash fons = nvg.fontManager.Fontstash;

            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            FonsQuad q = new();
            List<TextRow> rs = new(maxRows);
            float rowStartX = 0.0f;
            float rowWidth = 0.0f;
            float rowMinX = 0.0f;
            float rowMaxX = 0.0f;
            string rowStart = null;
            string rowEnd = null;
            string wordStart = null;
            float wordStartX = 0.0f;
            float wordMinX = 0.0f;
            string breakEnd = null;
            float breakWidth = 0.0f;
            float breakMaxX = 0.0f;
            CodepointType type, pType = CodepointType.Space;
            uint pCodepoint = 0;

            if (maxRows == 0)
            {
                return 0;
            }

            if (state.FontId == Fontstash.INVALID)
            {
                return 0;
            }

            if (@string == end || @string.Length == 0)
            {
                return 0;
            }

            fons.SetSize(state.FontSize * scale);
            fons.SetSpacing(state.LetterSpacing * scale);
            fons.SetBlur(state.FontBlur * scale);
            fons.SetAlign((int)state.TextAlign);
            fons.SetFont(state.FontId);

            breakRowWidth *= scale;

            fons.TextIterInit(out FonsTextIter iter, 0, 0, @string, end, FonsGlyphBitmap.Optional);
            FonsTextIter prevIter = iter;
            while (fons.TextIterNext(ref iter, ref q))
            {
                if (iter.prevGlyphIndex < 0 && nvg.fontManager.AllocTextAtlas())
                {
                    iter = prevIter;
                    fons.TextIterNext(ref iter, ref q);
                }
                prevIter = iter;
                switch (iter.codepoint)
                {
                    case 9: // \t
                    case 11: // \v
                    case 12: // \f
                    case 32: // \space
                    case 0x00a0: // NBSP
                        type = CodepointType.Space;
                        break;
                    case 10: // \n
                        type = pCodepoint == 13 ? CodepointType.Space : CodepointType.Newline;
                        break;
                    case 13: // \r
                        type = pCodepoint == 10 ? CodepointType.Space : CodepointType.Newline;
                        break;
                    case 0x0085: // NEL
                        type = CodepointType.Newline;
                        break;
                    default:
                        if ((iter.codepoint >= 0x4E00 && iter.codepoint <= 0x9FFF) ||
                            (iter.codepoint >= 0x3000 && iter.codepoint <= 0x30FF) ||
                            (iter.codepoint >= 0xFF00 && iter.codepoint <= 0xFFEF) ||
                            (iter.codepoint >= 0x1100 && iter.codepoint <= 0x11FF) ||
                            (iter.codepoint >= 0x3130 && iter.codepoint <= 0x318F) ||
                            (iter.codepoint >= 0xAC00 && iter.codepoint <= 0xD7AF))
                        {
                            type = CodepointType.CJKChar;
                        }
                        else
                        {
                            type = CodepointType.Char;
                        }
                        break;
                }

                if (type == CodepointType.Newline)
                {
                    rs.Add(new TextRow()
                    {
                        Start = rowStart ?? iter.str,
                        End = rowEnd ?? iter.str,
                        Width = rowWidth * invscale,
                        MinX = rowMinX * invscale,
                        MaxX = rowMaxX * invscale,
                        Next = iter.next
                    });

                    if (rs.Count >= maxRows)
                    {
                        rows = rs.ToArray();
                        return rs.Count;
                    }

                    breakEnd = rowStart;
                    breakWidth = 0.0f;
                    breakMaxX = 0.0f;

                    rowStart = null;
                    rowEnd = null;
                    rowWidth = 0.0f;
                    rowMinX = rowMaxX = 0.0f;
                }
                else
                {
                    if (rowStart == null)
                    {
                        if (type == CodepointType.Char || type == CodepointType.CJKChar)
                        {
                            rowStartX = iter.x;
                            rowStart = iter.str;
                            rowEnd = iter.next;
                            rowWidth = iter.nextx - rowStartX;
                            rowMinX = q.x0 - rowStartX;
                            rowMaxX = q.x1 - rowStartX;
                            wordStart = iter.str;
                            wordStartX = iter.x;
                            wordMinX = q.x0 - rowStartX;

                            breakEnd = rowStart;
                            breakWidth = 0.0f;
                            breakMaxX = 0.0f;
                        }
                    }
                    else
                    {
                        float nextWidth = iter.nextx - rowStartX;

                        if (type == CodepointType.Char || type == CodepointType.CJKChar)
                        {
                            rowEnd = iter.next;
                            rowWidth = iter.nextx - rowStartX;
                            rowMaxX = q.x1 - rowStartX;
                        }

                        if (((pType == CodepointType.Char || pType == CodepointType.CJKChar) && type == CodepointType.Space) || type == CodepointType.CJKChar)
                        {
                            breakEnd = iter.str;
                            breakWidth = rowWidth;
                            breakMaxX = rowMaxX;
                        }

                        if ((pType == CodepointType.Space && (type == CodepointType.Char || type == CodepointType.CJKChar)) || type == CodepointType.CJKChar)
                        {
                            wordStart = iter.str;
                            wordStartX = iter.x;
                            wordMinX = q.x0;
                        }

                        if ((type == CodepointType.Char || type == CodepointType.CJKChar) && nextWidth > breakRowWidth)
                        {
                            if (breakEnd == rowStart)
                            {
                                rs.Add(new TextRow()
                                {
                                    Start = rowStart,
                                    End = iter.str,
                                    Width = rowWidth * invscale,
                                    MinX = rowMinX * invscale,
                                    MaxX = rowMaxX * invscale,
                                    Next = iter.str
                                });

                                if (rs.Count >= maxRows)
                                {
                                    rows = rs.ToArray();
                                    return maxRows;
                                }

                                rowStartX = iter.x;
                                rowStart = iter.str;
                                rowEnd = iter.next;
                                rowWidth = iter.nextx - rowStartX;
                                rowMinX = q.x0 - rowStartX;
                                rowMaxX = q.x1 - rowStartX;
                                wordStart = iter.str;
                                wordStartX = iter.x;
                                wordMinX = q.x0 - rowStartX;
                            }
                            else
                            {
                                rs.Add(new TextRow()
                                {
                                    Start = rowStart,
                                    End = breakEnd,
                                    Width = breakWidth * invscale,
                                    MinX = rowMinX * invscale,
                                    MaxX = breakMaxX * invscale,
                                    Next = wordStart
                                });

                                if (rs.Count >= maxRows)
                                {
                                    rows = rs.ToArray();
                                    return maxRows;
                                }

                                rowStartX = wordStartX;
                                rowStart = wordStart;
                                rowEnd = iter.next;
                                rowWidth = iter.nextx - rowStartX;
                                rowMinX = wordMinX - rowStartX;
                                rowMaxX = q.x1 - rowStartX;
                            }

                            breakEnd = rowStart;
                            breakWidth = 0.0f;
                            breakMaxX = 0.0f;
                        }
                    }
                }

                pCodepoint = iter.codepoint;
                pType = type;
            }

            if (rowStart != null)
            {
                rs.Add(new TextRow()
                {
                    Start = rowStart,
                    End = rowEnd,
                    Width = rowWidth * invscale,
                    MinX = rowMinX * invscale,
                    MaxX = rowMaxX * invscale,
                    Next = end
                });
            }

            rows = rs.ToArray();
            return rs.Count;
        }

        public static int TextBreakLines(this Nvg nvg, string @string, float breakRowWidth, out TextRow[] rows, int maxRows)
            => TextBreakLines(nvg, @string, null, breakRowWidth, out rows, maxRows);

    }
}
