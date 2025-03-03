using System;
using System.IO;

namespace FontStash.NET
{

    public delegate void HandleError(FonsErrorCode error, int val);

    public sealed partial class Fontstash : IDisposable
    {

        public const int INVALID = -1;

        public const int SCRATCH_BUF_SIZE = 96000;
        public const int HASH_LUT_SIZE = 256;
        public const int INIT_FONTS = 4;
        public const int INIT_GLYPHS = 256;
        public const int INIT_ATLAS_NODES = 256;
        public const int VERTEX_COUNT = 1024;
        public const int MAX_STATES = 20;
        public const int MAX_FALLBACKS = 20;

        // Meta
        private FonsParams _params;
        private float _itw, _ith;

        // Texture
        private readonly int[] _dirtyRect = new int[4];
        private byte[] _texData;

        // Fonts
        private FonsFont[] _fonts;
        private FonsAtlas _atlas;
        private int _cfonts;
        private int _nfonts;

        // Rendering
        private readonly float[] _verts = new float[VERTEX_COUNT * 2];
        private readonly float[] _tcoords = new float[VERTEX_COUNT * 2];
        private readonly uint[] _colours = new uint[VERTEX_COUNT * 2];
        private int _nverts;

        // Scratch Buffer
        private byte[] _scratch;
        private int _nscratch;

        // States
        private readonly FonsState[] _states = new FonsState[MAX_STATES];
        private int _nstates;

        // Error Handling
        private HandleError _handleError;

        #region Constructor and Destructor
        public Fontstash(FonsParams @params)
        {
            _params = @params;

            _scratch = new byte[SCRATCH_BUF_SIZE];

            if (_params.renderCreate != null)
            {
                if (!_params.renderCreate.Invoke(_params.width, _params.height))
                {
                    Dispose();
                    throw new Exception("Failed to create fontstash!");
                }
            }

            _atlas = new FonsAtlas(_params.width, _params.height, INIT_ATLAS_NODES);

            _fonts = new FonsFont[INIT_FONTS];
            _cfonts = INIT_FONTS;
            _nfonts = 0;

            _itw = 1.0f / _params.width;
            _ith = 1.0f / _params.height;
            _texData = new byte[_params.width * _params.height];

            _dirtyRect[0] = _params.width;
            _dirtyRect[1] = _params.height;
            _dirtyRect[2] = 0;
            _dirtyRect[3] = 0;

            AddWhiteRect(2, 2);

            PushState();
            ClearState();
        }

        public void Dispose()
        {
            _params.renderDelete?.Invoke();
            Array.Clear(_fonts, 0, _nfonts);
            Array.Clear(_states, 0, _nstates);
            _atlas = null;
            GC.Collect();
        }
        #endregion

        #region Error
        public void SetErrorCallback(HandleError callback)
        {
            _handleError = callback;
        }
        #endregion

        #region Metrics
        public void GetAtlasSize(out int width, out int height)
        {
            width = _params.width;
            height = _params.height;
        }

        public bool ExpandAtlas(int width, int height)
        {
            width = Math.Max(width, _params.width);
            height = Math.Max(height, _params.height);

            if (width == _params.width && height == _params.height)
                return true;

            Flush();

            if (_params.renderResize?.Invoke(width, height) == false)
                return false;

            byte[] data = new byte[width * height];
            for (int i = 0; i < _params.height; i++)
            {
                int dstIdx = i * width;
                int srcIdx = i * _params.width;
                Array.Copy(_texData, srcIdx, data, dstIdx, _params.width);
                if (width > _params.width)
                {
                    Array.Fill<byte>(data, 0, dstIdx + _params.width, width - _params.width);
                }
            }
            if (height > _params.height)
            {
                Array.Fill<byte>(data, 0, _params.height * width, (height - _params.height) * width);
            }

            _texData = data;

            _atlas.Expand(width, height);

            int maxy = 0;
            for (int i = 0; i < _atlas.nnodes; i++)
                maxy = Math.Max(maxy, _atlas.nodes[i].y);
            _dirtyRect[0] = 0;
            _dirtyRect[1] = 0;
            _dirtyRect[2] = _params.width;
            _dirtyRect[3] = maxy;

            _params.width = width;
            _params.height = height;
            _itw = 1.0f / _params.width;
            _ith = 1.0f / _params.height;

            return true;
        }

        public bool ResetAtlas(int width, int height)
        {
            Flush();

            if (_params.renderResize != null)
            {
                if (_params.renderResize.Invoke(width, height) == false)
                {
                    return false;
                }
            }

            _atlas.Reset(width, height);

            _texData = new byte[width * height];

            _dirtyRect[0] = width;
            _dirtyRect[1] = height;
            _dirtyRect[2] = 0;
            _dirtyRect[3] = 0;

            for (int i = 0; i < _nfonts; i++)
            {
                FonsFont font = _fonts[i];
                font.nglyphs = 0;
                for (int j = 0; j < HASH_LUT_SIZE; j++)
                {
                    font.lut[j] = -1;
                }
            }

            _params.width = width;
            _params.height = height;
            _itw = 1.0f / _params.width;
            _ith = 1.0f / _params.height;

            AddWhiteRect(2, 2);

            return true;
        }
        #endregion

        #region Add Fonts
        public int AddFont(string name, string path, int fontIndex)
        {
            if (!File.Exists(path))
                return INVALID;

            byte[] data = File.ReadAllBytes(path);
            return AddFontMem(name, data, 1, fontIndex);
        }

        public int AddFont(string name, string path) => AddFont(name, path, 0);

        public int AddFontMem(string name, byte[] data, int freeData, int fontIndex)
        {
            int idx = AllocFont();
            if (idx == INVALID)
                return INVALID;

            FonsFont font = _fonts[idx];

            font.name = name;

            for ( int i = 0; i < HASH_LUT_SIZE; i++)
            {
                font.lut[i] = -1;
            }

            font.dataSize = data.Length;
            font.data = data;
            font.freeData = (byte)freeData;

            _nscratch = 0;
            if (FonsTt.LoadFont(font.font, data, fontIndex) == 0)
            {
                _nfonts--;
                return INVALID;
            }

            FonsTt.GetFontVMetrics(font.font, out int ascent, out int descent, out int lineGap);
            ascent += lineGap;
            int fh = ascent - descent;
            font.ascender = (float)ascent / (float)fh;
            font.descender = (float)descent / (float)fh;
            font.lineh = font.ascender - font.descender;

            return idx;
        }

        public int AddFontMem(string name, byte[] data, int freeData) => AddFontMem(name, data, freeData, 0);

        public int GetFontByName(string name)
        {
            for (int i = 0; i < _nfonts; i++)
            {
                if (_fonts[i].name == name)
                    return i;
            }
            return INVALID;
        }

        public bool AddFallbackFont(int @base, int fallback)
        {
            FonsFont baseFont = _fonts[@base];
            if (baseFont.nfallbacks < MAX_FALLBACKS)
            {
                baseFont.fallbacks[baseFont.nfallbacks++] = fallback;
                return true;
            }
            return false;
        }

        public void ResetFallbackFont(int @base)
        {
            FonsFont baseFont = _fonts[@base];
            baseFont.nfallbacks = 0;
            baseFont.nglyphs = 0;
            for (int i = 0; i < HASH_LUT_SIZE; i++)
            {
                baseFont.lut[i] = -1;
            }
        }
        #endregion

        #region State Handling
        public void PushState()
        {
            if (_nstates >= MAX_STATES)
            {
                _handleError?.Invoke(FonsErrorCode.StatesOverflow, 0);
                return;
            }

            if (_nstates > 0)
            {
                _states[_nstates - 1] = _states[_nstates].Copy();
            }
            _nstates++;
        }

        public void PopState()
        {
            if (_nstates <= 1)
            {
                _handleError.Invoke(FonsErrorCode.StatesUnderflow, 0);
                return;
            }
            _nstates--;
        }

        public void ClearState()
        {
            FonsState state = GetState();
            state.size = 12.0f;
            state.colour = 0xffffffff;
            state.font = 0;
            state.blur = 0;
            state.spacing = 0;
            state.align = (int)FonsAlign.Left | (int)FonsAlign.Baseline;
        }
        #endregion

        #region State Settings
        public void SetSize(float size)
        {
            GetState().size = size;
        }

        public void SetColour(uint colour)
        {
            GetState().colour = colour;
        }

        public void SetSpacing(float spacing)
        {
            GetState().spacing = spacing;
        }

        public void SetBlur(float blur)
        {
            GetState().blur = blur;
        }

        public void SetAlign(int align)
        {
            GetState().align = align;
        }

        public void SetFont(int font)
        {
            GetState().font = font;
        }
        #endregion

        #region Draw Text
        public float DrawText(float x, float y, string str, string end)
        {
            FonsState state = GetState();
            uint codepoint = 0;
            uint utf8state = 0;
            FonsGlyph glyph = null;
            int prevGlyphIndex = INVALID;
            short isize = (short)(state.size * 10.0f);
            short iblur = (short)state.blur;

            if (state.font < 0 || state.font >= _nfonts)
                return x;
            FonsFont font = _fonts[state.font];
            if (font.data == null)
                return x;

            float scale = FonsTt.GetPixelHeightScale(font.font, (float)isize / 10.0f);

            // Horizontal alignment
            if ((state.align & (int)FonsAlign.Left) != 0)
            {
                // empty
            }
            else if ((state.align & (int)FonsAlign.Right) != 0)
            {
                float width = TextBounds(x, y, str, end, out float[] _);
                x -= width;

            }
            else if ((state.align & (int)FonsAlign.Center) != 0)
            {
                float[] _ = Array.Empty<float>();
                float width = TextBounds(x, y, str, end, out float[] _);
                x -= width * 0.5f;
            }

            y += GetVertAlign(font, state.align, isize);

            for (int i = 0; i < str.Length; i++)
            {
                if (end != null && string.Compare(str, i, end, 0, end.Length) == 0)
                    break;
                
                if (Utf8.DecUtf8(ref utf8state, ref codepoint, str[i]) != 0)
                    continue;

                glyph = GetGlyph(font, codepoint, isize, iblur, FonsGlyphBitmap.Requiered);
                if (glyph != null)
                {
                    FonsQuad q = GetQuad(font, prevGlyphIndex, glyph, scale, state.spacing, ref x, ref y);

                    if (_nverts + 6 > VERTEX_COUNT)
                        Flush();

                    Vertex(q.x0, q.y0, q.s0, q.t0, state.colour);
                    Vertex(q.x1, q.y1, q.s1, q.t1, state.colour);
                    Vertex(q.x1, q.y0, q.s1, q.t0, state.colour);

                    Vertex(q.x0, q.y0, q.s0, q.t0, state.colour);
                    Vertex(q.x0, q.y1, q.s0, q.t1, state.colour);
                    Vertex(q.x1, q.y1, q.s1, q.t1, state.colour);
                }

                prevGlyphIndex = glyph != null ? glyph.index : -1;
            }

            Flush();

            return x;
        }

        public float DrawText(float x, float y, string str) => DrawText(x, y, str, null);
        #endregion

        #region Measure Text
        public float TextBounds(float x, float y, string str, string end, out float[] bounds)
        {
            FonsState state = GetState();
            uint codepoint = 0;
            uint utf8state = 0;
            FonsGlyph glyph = null;
            int prevGlyphIndex = -1;
            short isize = (short)(state.size * 10.0f);
            short iblur = (short)state.blur;
            bounds = new float[] { -1, -1, -1, -1 };

            if (state.font < 0 || state.font >= _nfonts)
                return x;
            FonsFont font = _fonts[state.font];
            if (font.data == null)
                return x;

            float scale = FonsTt.GetPixelHeightScale(font.font, (float)isize / 10.0f);

            y += GetVertAlign(font, state.align, isize);

            float minx = x, maxx = x;
            float miny = y, maxy = y;
            float startx = x;

            for (int i = 0; i < str.Length; i++)
            {
                if (end != null && string.Compare(str, i, end, 0, end.Length) == 0)
                    break;

                if (Utf8.DecUtf8(ref utf8state, ref codepoint, str[i]) != 0)
                    continue;
                
                glyph = GetGlyph(font, codepoint, isize, iblur, FonsGlyphBitmap.Optional);
                if (glyph != null)
                {
                    FonsQuad q = GetQuad(font, prevGlyphIndex, glyph, scale, state.spacing, ref x, ref y);
                    if (q.x0 < minx)
                        minx = q.x0;
                    if (q.x1 > maxx)
                        maxx = q.x1;
                    if ((_params.flags & (uint)FonsFlags.ZeroTopleft) != 0)
                    {
                        if (q.y0 < miny)
                            miny = q.y0;
                        if (q.y1 > maxy)
                            maxy = q.y1;
                    }
                    else
                    {
                        if (q.y1 < miny)
                            miny = q.y1;
                        if (q.y0 > maxy)
                            maxy = q.y0;
                    }
                }
                prevGlyphIndex = glyph != null ? glyph.index : -1;
            }

            float advance = x - startx;

            if ((state.align & (int)FonsAlign.Left) != 0)
            {
                // empty
            }
            else if ((state.align & (int)FonsAlign.Right) != 0)
            {
                minx -= advance;
                maxx -= advance;
            }
            else if ((state.align & (int)FonsAlign.Center) != 0)
            {
                minx -= advance * 0.5f;
                maxx -= advance * 0.5f;
            }

            bounds[0] = minx;
            bounds[1] = miny;
            bounds[2] = maxx;
            bounds[3] = maxy;

            return advance;
        }

        public void LineBounds(float y, out float miny, out float maxy)
        {
            miny = maxy = INVALID;
            FonsState state = GetState();

            if (state.font < 0 || state.font >= _nfonts)
                return;
            FonsFont font = _fonts[state.font];
            short isize = (short)(state.size * 10.0f);
            if (font.data == null)
                return;

            y += GetVertAlign(font, state.align, isize);

            if ((_params.flags & (byte)FonsFlags.ZeroTopleft) != 0)
            {
                miny = y - font.ascender * (float)isize / 10.0f;
                maxy = miny + font.lineh * isize / 10.0f;
            }
            else
            {
                miny = y + font.ascender * (float)isize / 10.0f;
                maxy = miny - font.lineh * isize / 10.0f;
            }
        }

        public void VertMetrics(out float ascender, out float descender, out float lineh)
        {
            FonsState state = GetState();
            ascender = descender = lineh = INVALID;

            if (state.font < 0 || state.font >= _nfonts)
                return;
            FonsFont font = _fonts[state.font];
            short isize = (short)(state.size * 10.0f);
            if (font.data == null)
                return;

            ascender = font.ascender * isize / 10.0f;
            descender = font.descender * isize / 10.0f;
            lineh = font.lineh * isize / 10.0f;
        }
        #endregion

        #region Text iterator
        public bool TextIterInit(out FonsTextIter iter, float x, float y, string str, string end, FonsGlyphBitmap bitmapOption)
        {
            FonsState state = GetState();

            iter = default;

            if (state.font < 0 || state.font >= _nfonts)
                return false;
            iter.font = _fonts[state.font];
            if (iter.font.data == null)
                return false;

            iter.isize = (short)(state.size * 10.0f);
            iter.iblur = (short)state.blur;
            iter.scale = FonsTt.GetPixelHeightScale(iter.font.font, (float)iter.isize / 10.0f);

            if ((state.align & (int)FonsAlign.Left) != 0)
            {
                // empty
            }
            else if ((state.align & (int)FonsAlign.Right) != 0)
            {
                float width = TextBounds(x, y, str, end, out float[] _);
                x -= width;
            }
            else if ((state.align & (int)FonsAlign.Center) != 0)
            {
                float width = TextBounds(x, y, str, end, out float[] _);
                x -= width * 0.5f;
            }

            y += GetVertAlign(iter.font, state.align, iter.isize);

            iter.x = iter.nextx = x;
            iter.y = iter.nexty = y;
            iter.spacing = state.spacing;
            iter.str = str;
            iter.next = str;
            iter.end = end;
            iter.codepoint = 0;
            iter.prevGlyphIndex = -1;
            iter.bitmapOption = bitmapOption;

            return true;
        }

        public bool TextIterNext(ref FonsTextIter iter, ref FonsQuad quad)
        {
            FonsGlyph glyph = null;
            string str = iter.next;
            iter.str = iter.next;

            if (str.Length == 0 || str == iter.end)
            {
                return false;
            }

            int i;
            for (i = 0; i < str.Length; i++)
            {
                if (iter.end != null && string.Compare(str, i, iter.end, 0, iter.end.Length) == 0)
                    break;

                if (Utf8.DecUtf8(ref iter.utf8state, ref iter.codepoint, str[i]) != 0)
                    continue;

                iter.x = iter.nextx;
                iter.y = iter.nexty;
                glyph = GetGlyph(iter.font, iter.codepoint, iter.isize, iter.iblur, iter.bitmapOption);
                if (glyph != null)
                    quad = GetQuad(iter.font, iter.prevGlyphIndex, glyph, iter.scale, iter.spacing, ref iter.nextx, ref iter.nexty);
                iter.prevGlyphIndex = glyph != null ? glyph.index : INVALID;
                break;
            }
            iter.next = str.Remove(0, i + 1);

            return true;
        }
        #endregion

        #region Pull Texture Changes
        public byte[] GetTextureData(out int width, out int height)
        {
            width = _params.width;
            height = _params.height;
            return _texData;
        }

        public bool ValidateTexture(out int[] dirty)
        {
            dirty = new int[] { -1, -1, -1, -1 };
            if (_dirtyRect[0] < _dirtyRect[2] && _dirtyRect[1] < _dirtyRect[3])
            {
                dirty[0] = _dirtyRect[0];
                dirty[1] = _dirtyRect[1];
                dirty[2] = _dirtyRect[2];
                dirty[3] = _dirtyRect[3];

                _dirtyRect[0] = _params.width;
                _dirtyRect[1] = _params.height;
                _dirtyRect[2] = 0;
                _dirtyRect[3] = 0;
                return true;
            }
            return false;
        }
        #endregion

        #region Debug
        public void DrawDebug(float x, float y)
        {
            int w = _params.width;
            int h = _params.height;
            float u = w == 0 ? 0 : (1.0f / w);
            float v = h == 0 ? 0 : (1.0f / h);

            if (_nverts + 6 + 6 > VERTEX_COUNT)
                Flush();

            Vertex(x + 0, y + 0, u, v, 0x0fffffff);
            Vertex(x + w, y + h, u, v, 0x0fffffff);
            Vertex(x + w, y + 0, u, v, 0x0fffffff);

            Vertex(x + 0, y + 0, u, v, 0x0fffffff);
            Vertex(x + 0, y + h, u, v, 0x0fffffff);
            Vertex(x + w, y + h, u, v, 0x0fffffff);

            Vertex(x + 0, y + 0, 0, 0, 0xffffffff);
            Vertex(x + w, y + h, 1, 1, 0xffffffff);
            Vertex(x + w, y + 0, 1, 0, 0xffffffff);

            Vertex(x + 0, y + 0, 0, 0, 0xffffffff);
            Vertex(x + 0, y + h, 0, 1, 0xffffffff);
            Vertex(x + w, y + h, 1, 1, 0xffffffff);

            for (int i = 0; i < _atlas.nnodes; i++)
            {
                FonsAtlasNode n = _atlas.nodes[i];

                if (_nverts + 6 > VERTEX_COUNT)
                    Flush();

                Vertex(x + n.x + 0, y + n.y + 0, u, v, 0xc00000ff);
                Vertex(x + n.x + n.width, y + n.y + 1, u, v, 0xc00000ff);
                Vertex(x + n.x + n.width, y + n.y + 0, u, v, 0xc00000ff);

                Vertex(x + n.x + 0, y + n.y + 0, u, v, 0xc00000ff);
                Vertex(x + n.x + 0, y + n.y + 1, u, v, 0xc00000ff);
                Vertex(x + n.x + n.width, y + n.y + 1, u, v, 0xc00000ff);
            }

            Flush();

        }
        #endregion

    }
}
