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

    /// <summary>
    /// <para>NanoVG allows you to load .ttf files and use the font to render text.</para>
    /// 
    /// <para>The apperance of the text can be defined by setting the current text style
    /// and by specifying the fill colour. Common text and font settings such as
    /// font size, letter spacing and text align are supported. Font bur allows you
    /// to create simple text effects such as drop shadows.</para>
    /// 
    /// <para>At render time the font face can be set based on the font handles or name.</para>
    /// 
    /// <para>Font measure functions return values in local space, the calculations arre
    /// carried in the same resolution as the final rendering. This is because
    /// the text glyph positions are snapped to the nearest pixel sharp rendering.</para>
    /// 
    /// <para>The local space means that values are not rotated or scale as per the current
    /// transformation. For example if you set font size to 12, which would mean that
    /// line height is 16, then regardless of the current scaling and rotation, the
    /// returned line height is always 16. Some measures may vary because of the scaling
    /// since aforementioned pixel snapping.</para>
    /// 
    /// <example>While this may sound a little odd, the setup allows you to always render
    /// the same way regardless of scaling. I.e. following works regardless of scaling:
    /// <code>
    ///     string txt = "Text me up.";
    ///     nvg.TextBounds(x, y, txt, out Rectangle bounds);
    ///     nvg.BeginPath();
    ///     nvg.Rect(bounds.Origin.X, bounds.Origin.Y, bounds.Size.X, bounds.Size.Y);
    ///     nvg.Fill();
    /// </code>
    /// Note: currently only solid colour fill is supported for text.</example>
    /// </summary>
    public static class NvgText
    {

        /// <summary>
        /// Creates font by loading it from the disk from specified filename.
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public static int CreateFont(this Nvg nvg, string name, string fileName)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFont(name, fileName, 0);
        }

        /// <param name="fontIndex">Specifies which font face to load from a .ttf/.ttc file</param>
        public static int CreateFontAtIndex(this Nvg nvg, string name, string fileName, int fontIndex)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFont(name, fileName, fontIndex);
        }

        /// <summary>
        /// Creates a font by loading it from the specified memory chunk.
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public static int CreateFontMem(this Nvg nvg, string name, byte[] data, int freeData)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFontMem(name, data, freeData);
        }

        /// <param name="fontIndex">Specifies which font face to load from a .ttf/.ttc file</param>
        public static int CreateFontMemAtIndex(this Nvg nvg, string name, byte[] data, int freeData, int fontIndex)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFontMem(name, data, freeData, fontIndex);
        }

        /// <summary>
        /// Finds a loaded font from specified name.
        /// </summary>
        /// <returns>Handle to it, or -1 if the font is not found.</returns>
        public static int FindFont(this Nvg nvg, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return -1;
            }

            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.GetFontByName(name);
        }

        /// <summary>
        /// Adds a fallback font by handle.
        /// </summary>
        public static bool AddFallbackFontId(this Nvg nvg, int baseFont, int fallbackFont)
        {
            if (baseFont == -1 || fallbackFont == -1)
            {
                return false;
            }

            Fontstash fons = nvg.fontManager.Fontstash;
            return fons.AddFallbackFont(baseFont, fallbackFont);
        }

        /// <summary>
        /// Adds a fallback font by name.
        /// </summary>
        public static bool AddFallbackFont(this Nvg nvg, string baseFont, string fallbackFont)
        {
            return AddFallbackFontId(nvg, FindFont(nvg, baseFont), FindFont(nvg, fallbackFont));
        }

        /// <summary>
        /// Resets fallback fonts by handle.
        /// </summary>
        public static void ResetFallbackFontsId(this Nvg nvg, int baseFont)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            fons.ResetFallbackFont(baseFont);
        }

        /// <summary>
        /// Resets fallback fonts by name.
        /// </summary>
        public static void ResetFallbackFonts(this Nvg nvg, string baseFont)
        {
            ResetFallbackFontsId(nvg, FindFont(nvg, baseFont));
        }

        /// <summary>
        /// Sets the font size of current text style.
        /// </summary>
        public static void FontSize(this Nvg nvg, float size)
        {
            nvg.stateStack.CurrentState.FontSize = size;
        }

        /// <summary>
        /// Sets the blur of current text style.
        /// </summary>
        public static void FontBlur(this Nvg nvg, float blur)
        {
            nvg.stateStack.CurrentState.FontBlur = blur;
        }

        /// <summary>
        /// Sets the letter spacing of current text style.
        /// </summary>
        public static void TextLetterSpacing(this Nvg nvg, float spacing)
        {
            nvg.stateStack.CurrentState.LetterSpacing = spacing;
        }

        /// <summary>
        /// Sets the proportinal line height of current text style. The line height is specified as multiple of font size.
        /// </summary>
        public static void TextLineHeight(this Nvg nvg, float lineHeight)
        {
            nvg.stateStack.CurrentState.LineHeight = lineHeight;
        }

        /// <summary>
        /// Sets the text align of current text style, <see cref="Align"/> for options.
        /// </summary>
        public static void TextAlign(this Nvg nvg, Align align)
        {
            nvg.stateStack.CurrentState.TextAlign = align;
        }

        /// <summary>
        /// Sets the font face based on specified id of current text style.
        /// </summary>
        public static void FontFaceId(this Nvg nvg, int font)
        {
            nvg.stateStack.CurrentState.FontId = font;
        }

        /// <summary>
        /// Sets the font face based on specified name of current text style.
        /// </summary>
        public static void FontFace(this Nvg nvg, string font)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            nvg.stateStack.CurrentState.FontId = fons.GetFontByName(font);
        }

        /// <summary>
        /// Draws text string at specified location. Only the sub-string up to the end is drawn.
        /// </summary>
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
            nvg.fontManager.RenderText(vertices);

            return iter.nextx / scale;
        }

        /// <summary>
        /// Draws text string at specified location.
        /// </summary>
        public static float Text(this Nvg nvg, Vector2D<float> pos, string @string)
            => Text(nvg, pos, @string, null);

        /// <inheritdoc cref="Text(Nvg, Vector2D{float}, string, string)"/>
        public static float Text(this Nvg nvg, float x, float y, string @string, string end)
            => Text(nvg, new Vector2D<float>(x, y), @string, end);

        /// <inheritdoc cref="Text(Nvg, Vector2D{float}, string)"/>
        public static float Text(this Nvg nvg, float x, float y, string @string)
            => Text(nvg, new Vector2D<float>(x, y), @string, null);

        /// <summary>
        /// Draws multi-line text string at specified location wrapped at the specified width. Only the sub-string up to the end is drawn.
        /// White space is stripped at the beginning of the rows, the text is split at word boundries or when new-line characters are encountered.
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static void TextBox(this Nvg nvg, Vector2D<float> pos, float breakRowWidth, string @string, string end)
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
                        _ = Text(nvg, pos, rows[i].Start, rows[i].End.Length > 0 ? rows[i].End : null) ;
                    }
                    else if (hAlign.HasFlag(Align.Centre))
                    {
                        _ = Text(nvg, pos.X + breakRowWidth * 0.5f, pos.Y - rows[i].Width * 0.5f, rows[i].Start, rows[i].End.Length > 0 ? rows[i].End : null);
                    }
                    else if (hAlign.HasFlag(Align.Right))
                    {
                        _ = Text(nvg, pos.X + breakRowWidth - rows[i].Width, pos.Y, rows[i].Start, rows[i].End.Length > 0 ? rows[i].End : null);
                    }
                    pos.Y += lineh * state.LineHeight;
                }
                @string = rows[rowCount - 1].Next;
            }

            state.TextAlign = oldAlign;
        }

        /// <summary>
        /// Draws multi-line text string at specified location wrapped at the specified width. White space is stripped at the beginning of the rows,
        /// the text is split at word boundries or when new-line characters are encountered. Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static void TextBox(this Nvg nvg, Vector2D<float> pos, float breakRowWidth, string @string)
            => TextBox(nvg, pos, breakRowWidth, @string, null);

        /// <inheritdoc cref="TextBox(Nvg, Vector2D{float}, float, string, string)"/>
        public static void TextBox(this Nvg nvg, float x, float y, float breakRowWidth, string @string, string end)
            => TextBox(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, end);

        /// <inheritdoc cref="TextBox(Nvg, Vector2D{float}, float, string)"/>
        public static void TextBox(this Nvg nvg, float x, float y, float breakRowWidth, string @string)
            => TextBox(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, null);

        /// <summary>
        /// Measures the specified text string. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public static float TextBounds(this Nvg nvg, Vector2D<float> pos, string @string, string end, out Rectangle<float> bounds)
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

            float width = fons.TextBounds(pos.X * scale, pos.Y * scale, @string, end, out float[] bs);
            if (bs != null)
            {
                fons.LineBounds(pos.Y * scale, out bs[1], out bs[3]);
                bounds = new Rectangle<float>()
                {
                    Origin = new Vector2D<float>(bs[0] * invscale, bs[1] * invscale),
                    Size = new Vector2D<float>((bs[2] - bs[0]) * invscale, (bs[3] - bs[1]) * invscale)
                };
            }

            return width * invscale;
        }

        /// <inheritdoc cref="TextBounds(Nvg, Vector2D{float}, string, string, out Rectangle{float})"/>
        public static float TextBounds(this Nvg nvg, Vector2D<float> pos, string @string, out Rectangle<float> bounds)
            => TextBounds(nvg, pos, @string, null, out bounds);

        /// <inheritdoc cref="TextBounds(Nvg, Vector2D{float}, string, string, out Rectangle{float})"/>
        public static float TextBounds(this Nvg nvg, float x, float y, string @string, string end, out Rectangle<float> bounds)
            => TextBounds(nvg, new Vector2D<float>(x, y), @string, end, out bounds);

        /// <inheritdoc cref="TextBounds(Nvg, Vector2D{float}, string, string, out Rectangle{float})"/>
        public static float TextBounds(this Nvg nvg, float x, float y, string @string, out Rectangle<float> bounds)
            => TextBounds(nvg, new Vector2D<float>(x, y), @string, null, out bounds);

        /// <summary>
        /// Measures the specified multi-text string.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public static void TextBoxBounds(this Nvg nvg, Vector2D<float> pos, float breakRowWidth, string @string, string end, out Rectangle<float> bounds)
        {
            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            int nrows;
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

            float minX = pos.X, maxX = pos.X;
            float minY = pos.Y, maxY = pos.Y;

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
                    rMinX = pos.X + rows[i].MinX + dx;
                    rMaxX = pos.X + rows[i].MaxX + dx;
                    minX = MathF.Min(minX, rMinX);
                    maxX = MathF.Max(maxX, rMaxX);

                    minY = MathF.Min(minY, pos.Y + rMinY);
                    maxY = MathF.Max(maxY, pos.Y + rMaxY);

                    pos.Y += lineh * state.LineHeight;
                }
                @string = rows[nrows - 1].Next;
            }

            state.TextAlign = oldAlign;

            bounds = new Rectangle<float>(new Vector2D<float>(minX, minY), new Vector2D<float>(maxX, maxY) - new Vector2D<float>(minX, minY));
        }

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2D{float}, float, string, string, out Rectangle{float})"/>
        public static void TextBoxBounds(this Nvg nvg, Vector2D<float> pos, float breakRowWidth, string @string, out Rectangle<float> bounds)
            => TextBoxBounds(nvg, pos, breakRowWidth, @string, null, out bounds);

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2D{float}, float, string, string, out Rectangle{float})"/>
        public static void TextBoxBounds(this Nvg nvg, float x, float y, float breakRowWidth, string @string, string end, out Rectangle<float> bounds)
            => TextBoxBounds(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, end, out bounds);

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2D{float}, float, string, string, out Rectangle{float})"/>
        public static void TextBoxBounds(this Nvg nvg, float x, float y, float breakRowWidth, string @string, out Rectangle<float> bounds)
            => TextBoxBounds(nvg, new Vector2D<float>(x, y), breakRowWidth, @string, null, out bounds);

        /// <summary>
        /// Calculates the glyph x positions of the specified text. Only the sub-string will be used.<br/>
        /// Measures values are returned in local coordinate space.
        /// </summary>
        public static int TextGlyphPositions(this Nvg nvg, Vector2D<float> pos, string @string, string end, out GlyphPosition[] positions, int maxRows)
        {
            positions = new GlyphPosition[maxRows];

            Fontstash fons = nvg.fontManager.Fontstash;
            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            FonsQuad q = new();
            int npos = 0;

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

            fons.TextIterInit(out FonsTextIter iter, pos.X * scale, pos.Y * scale, @string, end, FonsGlyphBitmap.Optional);
            FonsTextIter prevIter = iter;
            while (fons.TextIterNext(ref iter, ref q))
            {
                if (iter.prevGlyphIndex < 0 && nvg.fontManager.AllocTextAtlas())
                {
                    iter = prevIter;
                    fons.TextIterNext(ref iter, ref q);
                }
                prevIter = iter;
                positions[npos++] = new GlyphPosition(iter.str, iter.x * invscale, MathF.Min(iter.x, q.x0) * invscale, MathF.Max(iter.nextx, q.x1) * invscale);
                if (npos >= maxRows)
                {
                    return npos;
                }
            }

            return npos;
        }

        /// <inheritdoc cref="TextGlyphPositions(Nvg, Vector2D{float}, string, string, out GlyphPosition[], int)"/>
        public static int TextGlyphPositions(this Nvg nvg, float x, float y, string @string, string end, out GlyphPosition[] positions, int maxRows)
            => TextGlyphPositions(nvg, new Vector2D<float>(x, y), @string, end, out positions, maxRows);


        /// <summary>
        /// Returns the vertical metrics based on the current text style.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
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

        /// <summary>
        /// Breaks the specified text into lines. Only the sub-string will be used.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static int TextBreakLines(this Nvg nvg, string @string, string end, float breakRowWidth, out TextRow[] rows, int maxRows)
        {
            rows = new TextRow[maxRows];

            Fontstash fons = nvg.fontManager.Fontstash;

            State state = nvg.stateStack.CurrentState;
            float scale = nvg.fontManager.GetFontScale() * nvg.pixelRatio.DevicePxRatio;
            float invscale = 1.0f / scale;
            FonsQuad q = new();
            int nrows = 0;
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
                    rows[nrows++] = new TextRow()
                    {
                        Start = rowStart ?? iter.str,
                        End = rowEnd ?? iter.str,
                        Width = rowWidth * invscale,
                        MinX = rowMinX * invscale,
                        MaxX = rowMaxX * invscale,
                        Next = iter.next
                    };

                    if (nrows >= maxRows)
                    {
                        return nrows;
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
                                rows[nrows++] = new TextRow()
                                {
                                    Start = rowStart,
                                    End = iter.str,
                                    Width = rowWidth * invscale,
                                    MinX = rowMinX * invscale,
                                    MaxX = rowMaxX * invscale,
                                    Next = iter.str
                                };

                                if (nrows >= maxRows)
                                {
                                    return nrows;
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
                                rows[nrows++] = new TextRow()
                                {
                                    Start = rowStart,
                                    End = breakEnd,
                                    Width = breakWidth * invscale,
                                    MinX = rowMinX * invscale,
                                    MaxX = breakMaxX * invscale,
                                    Next = wordStart
                                };

                                if (nrows >= maxRows)
                                {
                                    return nrows;
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
                rows[nrows++] = new TextRow()
                {
                    Start = rowStart,
                    End = rowEnd,
                    Width = rowWidth * invscale,
                    MinX = rowMinX * invscale,
                    MaxX = rowMaxX * invscale,
                    Next = end
                };
            }

            return nrows;
        }

        /// <summary>
        /// Breaks the specified text into lines.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static int TextBreakLines(this Nvg nvg, string @string, float breakRowWidth, out TextRow[] rows, int maxRows)
            => TextBreakLines(nvg, @string, null, breakRowWidth, out rows, maxRows);

    }
}
