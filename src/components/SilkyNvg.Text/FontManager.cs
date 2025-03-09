using FontStashSharp;
using FontStashSharp.RichText;
using SilkyNvg.Text.Platform;
using System.Collections.Generic;
using System.Numerics;

namespace SilkyNvg.Text
{
    internal sealed class FontManager
    {

        private const int KERNEL_SIZE = 2;
        private const int ATLAS_SIZE = 1024;

        private readonly Dictionary<Font, FontSystem> _fonts = [];

        private readonly FontRenderer _fontRenderer;

        private readonly Nvg _nvg;

        internal FontManager(Nvg nvg)
        {
            _nvg = nvg;

            _fontRenderer = new FontRenderer(_nvg);
        }

        internal Font CreateFont(string name)
        {
            var settings = new FontSystemSettings()
            {
                KernelWidth = KERNEL_SIZE,
                KernelHeight = KERNEL_SIZE,

                TextureWidth = ATLAS_SIZE,
                TextureHeight = ATLAS_SIZE
            };
            var system = new FontSystem(settings);

            var group = new Font(name, _fonts.Count);
            _fonts.Add(group, system);

            return group;
        }

        internal bool AddFontData(Font font, byte[] data)
        {
            if (!_fonts.TryGetValue(font, out FontSystem system))
            {
                return false;
            }
            system.AddFont(data);
            return true;
        }

        internal Font? GetFontByName(string name)
        {
            foreach (Font font in _fonts.Keys)
            {
                if (font.Name == name)
                {
                    return font;
                }
            }
            return null;
        }

        internal float Write(Font font, string text, float size, Vector2 position)
        {
            if (!_fonts.TryGetValue(font, out FontSystem system))
            {
                return float.NaN;
            }

            var systemFont = system.GetFont(size);
            _fontRenderer.PrepareDraw();
            float result = systemFont.DrawText(_fontRenderer, text, position, FSColor.White);
            _fontRenderer.FinishDraw();
            return result;
        }

        internal void Reset(Font font)
        {
            if (!_fonts.TryGetValue(font, out FontSystem system))
            {
                return;
            }
            system.Reset();
        }

    }
}
