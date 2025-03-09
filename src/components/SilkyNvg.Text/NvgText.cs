using SilkyNvg.Common;
using SilkyNvg.Core.States;
using SilkyNvg.Rendering;
using SilkyNvg.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

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

        private static readonly ConditionalWeakTable<Nvg, FontManager> fontManagers = [];

        private static FontManager FontManager(this Nvg nvg)
            => fontManagers.GetValue(nvg, nvg => new FontManager(nvg));

        /// <summary>
        /// Creates an empty font. Add fonts using <see cref="AddFallbackFont(Nvg, string, string)"/>
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public static Font CreateEmpty(this Nvg nvg, string name)
        {
            var manager = nvg.FontManager();
            return manager.CreateFont(name);
        }

        /// <summary>
        /// Creates font by loading it from the disk from specified filename.
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public static Font CreateFont(this Nvg nvg, string name, string fileName)
            => nvg.CreateFontMem(name, File.ReadAllBytes(fileName));

        /// <summary>
        /// Creates a font by loading it from the specified memory chunk.
        /// </summary>
        /// <returns>Handle to the font.</returns>
        public static Font CreateFontMem(this Nvg nvg, string name, byte[] data)
        {
            var manager = nvg.FontManager();
            Font font = manager.CreateFont(name);
            if (!manager.AddFontData(font, data))
            {
                throw new InvalidDataException("Failed to add font data");
            }
            return font;
        }

        /// <summary>
        /// Finds a loaded font from specified name.
        /// </summary>
        /// <returns>Handle to it, or null if the font is not found.</returns>
        public static Font? FindFont(this Nvg nvg, string name)
        {
            var manager = nvg.FontManager();
            return manager.GetFontByName(name);
        }

        /// <summary>
        /// Adds a fallback font from file.
        /// </summary>
        public static bool AddFallbackFont(this Nvg nvg, Font font, string fileName)
            => nvg.AddFallbackFontMem(font, File.ReadAllBytes(fileName));

        /// <summary>
        /// Adds a fallback font from the specified memory chunk.
        /// </summary>
        public static bool AddFallbackFontMem(this Nvg nvg, Font font, byte[] data)
        {
            var manager = nvg.FontManager();
            return manager.AddFontData(font, data);
        }

        /// <summary>
        /// Adds a fallback font from file.
        /// </summary>
        public static bool AddFallbackFont(this Nvg nvg, string name, string fileName)
            => nvg.AddFallbackFontMem(name, File.ReadAllBytes(fileName));

        /// <summary>
        /// Adds a fallback font from the specified memory chunk.
        /// </summary>
        public static bool AddFallbackFontMem(this Nvg nvg, string name, byte[] data)
        {
            var manager = nvg.FontManager();
            Font? font = manager.GetFontByName(name);
            if (!font.HasValue)
            {
                return false;
            }
            return nvg.AddFallbackFontMem(font.Value, data);
        }

        /// <summary>
        /// Resets fallback fonts by handle.
        /// </summary>
        public static void ResetFont(this Nvg nvg, Font font)
        {
            var manager = nvg.FontManager();
            manager.Reset(font);
        }

        /// <summary>
        /// Resets fallback fonts by name.
        /// </summary>
        public static void ResetFont(this Nvg nvg, string name)
        {
            var manager = nvg.FontManager();
            Font? font = manager.GetFontByName(name);
            if (!font.HasValue)
            {
                return;
            }
            nvg.ResetFont(font.Value);
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

        }

        /// <summary>
        /// Sets the letter spacing of current text style.
        /// </summary>
        public static void TextLetterSpacing(this Nvg nvg, float spacing)
        {

        }

        /// <summary>
        /// Sets the proportinal line height of current text style. The line height is specified as multiple of font size.
        /// </summary>
        public static void TextLineHeight(this Nvg nvg, float lineHeight)
        {

        }

        /// <summary>
        /// Sets the text align of current text style, <see cref="Align"/> for options.
        /// </summary>
        public static void TextAlign(this Nvg nvg, Align align)
        {

        }

        /// <summary>
        /// Sets the font face based on specified id of current text style.
        /// </summary>
        public static void FontFace(this Nvg nvg, Font font)
            => nvg.stateStack.CurrentState.CurrentFont = font;

        /// <summary>
        /// Sets the font face based on specified name of current text style.
        /// </summary>
        public static void FontFace(this Nvg nvg, string font)
            => nvg.FontFace(nvg.FontManager().GetFontByName(font) ?? Font.None);

        /// <summary>
        /// Draws text string at specified location. Only the sub-string up to the end is drawn.
        /// </summary>
        public static float Text(this Nvg nvg, Vector2 pos, string @string, string end)
        {
            var manager = nvg.FontManager();
            var state = nvg.stateStack.CurrentState;

            if (end != null)
            {
                int cutoff = @string.IndexOf(end);
                @string = @string[..cutoff];
            }


            return manager.Write(state.CurrentFont, @string, state.FontSize, pos);
        }

        /// <inheritdoc cref="Text(Nvg, Vector2, string, string)"/>
        public static float Text(this Nvg nvg, PointF pos, string @string, string end)
            => Text(nvg, pos.ToVector2(), @string, end);

        /// <inheritdoc cref="Text(Nvg, Vector2, string, string)"/>
        public static float Text(this Nvg nvg, float x, float y, string @string, string end)
            => Text(nvg, new Vector2(x, y), @string, end);

        /// <summary>
        /// Draws text string at specified location.
        /// </summary>
        public static float Text(this Nvg nvg, Vector2 pos, string @string)
            => Text(nvg, pos, @string, null);

        /// <inheritdoc cref="Text(Nvg, Vector2, string)"/>
        public static float Text(this Nvg nvg, PointF pos, string @string)
            => Text(nvg, pos.ToVector2(), @string, null);

        /// <inheritdoc cref="Text(Nvg, Vector2, string)"/>
        public static float Text(this Nvg nvg, float x, float y, string @string)
            => Text(nvg, new Vector2(x, y), @string, null);

        /// <summary>
        /// Draws multi-line text string at specified location wrapped at the specified width. Only the sub-string up to the end is drawn.
        /// White space is stripped at the beginning of the rows, the text is split at word boundries or when new-line characters are encountered.
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static void TextBox(this Nvg nvg, Vector2 pos, float breakRowWidth, string @string, string end)
        {

        }

        /// <inheritdoc cref="TextBox(Nvg, Vector2, float, string, string)"/>
        public static void TextBox(this Nvg nvg, PointF pos, float breakRowWidth, string @string, string end)
            => TextBox(nvg, pos.ToVector2(), breakRowWidth, @string, end);

        /// <inheritdoc cref="TextBox(Nvg, Vector2, float, string, string)"/>
        public static void TextBox(this Nvg nvg, float x, float y, float breakRowWidth, string @string, string end)
            => TextBox(nvg, new Vector2(x, y), breakRowWidth, @string, end);

        /// <summary>
        /// Draws multi-line text string at specified location wrapped at the specified width. White space is stripped at the beginning of the rows,
        /// the text is split at word boundries or when new-line characters are encountered. Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static void TextBox(this Nvg nvg, Vector2 pos, float breakRowWidth, string @string)
            => TextBox(nvg, pos, breakRowWidth, @string, null);

        /// <inheritdoc cref="TextBox(Nvg, Vector2, float, string)"/>
        public static void TextBox(this Nvg nvg, PointF pos, float breakRowWidth, string @string)
            => TextBox(nvg, pos.ToVector2(), breakRowWidth, @string, null);

        /// <inheritdoc cref="TextBox(Nvg, Vector2, float, string)"/>
        public static void TextBox(this Nvg nvg, float x, float y, float breakRowWidth, string @string)
            => TextBox(nvg, new Vector2(x, y), breakRowWidth, @string, null);

        /// <summary>
        /// Measures the specified text string. Parameter bounds contains the bounds of the text.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        /// <param name="bounds">Contains the bounds of the text when returned.</param>
        /// <returns>The horizontal advance of the measured text (i.e. where the next character should be drawn).</returns>
        public static float TextBounds(this Nvg nvg, Vector2 pos, string @string, string end, out RectangleF bounds)
        {
            bounds = default;
            return float.NaN;
        }

        /// <inheritdoc cref="TextBounds(Nvg, Vector2, string, string, out RectangleF)"/>
        public static float TextBounds(this Nvg nvg, PointF pos, string @string, string end, out RectangleF bounds)
            => TextBounds(nvg, pos.ToVector2(), @string, end, out bounds);

        /// <inheritdoc cref="TextBounds(Nvg, Vector2, string, string, out RectangleF)"/>
        public static float TextBounds(this Nvg nvg, float x, float y, string @string, string end, out RectangleF bounds)
            => TextBounds(nvg, new Vector2(x, y), @string, end, out bounds);

        /// <inheritdoc cref="TextBounds(Nvg, Vector2, string, string, out RectangleF)"/>
        public static float TextBounds(this Nvg nvg, Vector2 pos, string @string, out RectangleF bounds)
            => TextBounds(nvg, pos, @string, null, out bounds);

        /// <inheritdoc cref="TextBounds(Nvg, Vector2, string, string, out RectangleF)"/>
        public static float TextBounds(this Nvg nvg, PointF pos, string @string, out RectangleF bounds)
            => TextBounds(nvg, pos, @string, null, out bounds);

        /// <inheritdoc cref="TextBounds(Nvg, Vector2, string, string, out RectangleF)"/>
        public static float TextBounds(this Nvg nvg, float x, float y, string @string, out RectangleF bounds)
            => TextBounds(nvg, new Vector2(x, y), @string, null, out bounds);

        /// <summary>
        /// Measures the specified multi-text string.<br/>
        /// Measured values are returned in local space.
        /// </summary>
        /// <param name="bounds">Contains the bounds box of the multi-text when returned.</param>
        public static void TextBoxBounds(this Nvg nvg, Vector2 pos, float breakRowWidth, string @string, string end, out RectangleF bounds)
        {
            bounds = default;
        }

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2, float, string, string, out RectangleF)"/>
        public static void TextBoxBounds(this Nvg nvg, PointF pos, float breakRowWidth, string @string, string end, out RectangleF bounds)
            => TextBoxBounds(nvg, pos.ToVector2(), breakRowWidth, @string, end, out bounds);

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2, float, string, string, out RectangleF)"/>
        public static void TextBoxBounds(this Nvg nvg, float x, float y, float breakRowWidth, string @string, string end, out RectangleF bounds)
            => TextBoxBounds(nvg, new Vector2(x, y), breakRowWidth, @string, end, out bounds);

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2, float, string, string, out RectangleF)"/>
        public static void TextBoxBounds(this Nvg nvg, Vector2 pos, float breakRowWidth, string @string, out RectangleF bounds)
            => TextBoxBounds(nvg, pos, breakRowWidth, @string, null, out bounds);

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2, float, string, string, out RectangleF)"/>
        public static void TextBoxBounds(this Nvg nvg, PointF pos, float breakRowWidth, string @string, out RectangleF bounds)
            => TextBoxBounds(nvg, pos, breakRowWidth, @string, null, out bounds);

        /// <inheritdoc cref="TextBoxBounds(Nvg, Vector2, float, string, string, out RectangleF)"/>
        public static void TextBoxBounds(this Nvg nvg, float x, float y, float breakRowWidth, string @string, out RectangleF bounds)
            => TextBoxBounds(nvg, new Vector2(x, y), breakRowWidth, @string, null, out bounds);

        /// <summary>
        /// Calculates the glyph x positions of the specified text. Only the sub-string will be used.<br/>
        /// Measures values are returned in local coordinate space.
        /// </summary>
        public static int TextGlyphPositions(this Nvg nvg, Vector2 pos, string @string, string end, out GlyphPosition[] positions, int maxRows)
        {
            positions = [];
            return 0;
        }

        /// <inheritdoc cref="TextGlyphPositions(Nvg, Vector2, string, string, out GlyphPosition[], int)"/>
        public static int TextGlyphPositions(this Nvg nvg, PointF pos, string @string, string end, out GlyphPosition[] positions, int maxRows)
            => TextGlyphPositions(nvg, pos.ToVector2(), @string, end, out positions, maxRows);

        /// <inheritdoc cref="TextGlyphPositions(Nvg, Vector2, string, string, out GlyphPosition[], int)"/>
        public static int TextGlyphPositions(this Nvg nvg, float x, float y, string @string, string end, out GlyphPosition[] positions, int maxRows)
            => TextGlyphPositions(nvg, new Vector2(x, y), @string, end, out positions, maxRows);


        /// <summary>
        /// Returns the vertical metrics based on the current text style.<br/>
        /// Measured values are returned in local coordinate space.
        /// </summary>
        public static void TextMetrics(this Nvg nvg, out float ascender, out float descender, out float lineh)
        {
            ascender = descender = lineh = float.NaN;
        }

        /// <summary>
        /// Breaks the specified text into lines. Only the sub-string will be used.<br/>
        /// White space is stripped at the beginning of the rows, the text is split at word boundaries or when new-line characters are encountered.<br/>
        /// Words longer than the max width are slit at nearest character (i.e. no hyphenation).
        /// </summary>
        public static int TextBreakLines(this Nvg nvg, string @string, string end, float breakRowWidth, out TextRow[] rows, int maxRows)
        {
            rows = [];
            return 0;
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
