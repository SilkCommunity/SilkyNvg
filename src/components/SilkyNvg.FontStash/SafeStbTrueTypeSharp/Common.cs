using System;
using System.Runtime.InteropServices;
using static StbTrueTypeSharp.PackContext;

namespace StbTrueTypeSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	static partial class Common
	{
		public const int STBTT_vmove = 1;
		public const int STBTT_vline = 2;
		public const int STBTT_vcurve = 3;
		public const int STBTT_vcubic = 4;

		public const int STBTT_PLATFORM_ID_UNICODE = 0;
		public const int STBTT_PLATFORM_ID_MAC = 1;
		public const int STBTT_PLATFORM_ID_ISO = 2;
		public const int STBTT_PLATFORM_ID_MICROSOFT = 3;

		public const int STBTT_UNICODE_EID_UNICODE_1_0 = 0;
		public const int STBTT_UNICODE_EID_UNICODE_1_1 = 1;
		public const int STBTT_UNICODE_EID_ISO_10646 = 2;
		public const int STBTT_UNICODE_EID_UNICODE_2_0_BMP = 3;
		public const int STBTT_UNICODE_EID_UNICODE_2_0_FULL = 4;

		public const int STBTT_MS_EID_SYMBOL = 0;
		public const int STBTT_MS_EID_UNICODE_BMP = 1;
		public const int STBTT_MS_EID_SHIFTJIS = 2;
		public const int STBTT_MS_EID_UNICODE_FULL = 10;

		public const int STBTT_MAC_EID_ROMAN = 0;
		public const int STBTT_MAC_EID_ARABIC = 4;
		public const int STBTT_MAC_EID_JAPANESE = 1;
		public const int STBTT_MAC_EID_HEBREW = 5;
		public const int STBTT_MAC_EID_CHINESE_TRAD = 2;
		public const int STBTT_MAC_EID_GREEK = 6;
		public const int STBTT_MAC_EID_KOREAN = 3;
		public const int STBTT_MAC_EID_RUSSIAN = 7;

		public const int STBTT_MS_LANG_ENGLISH = 0x0409;
		public const int STBTT_MS_LANG_ITALIAN = 0x0410;
		public const int STBTT_MS_LANG_CHINESE = 0x0804;
		public const int STBTT_MS_LANG_JAPANESE = 0x0411;
		public const int STBTT_MS_LANG_DUTCH = 0x0413;
		public const int STBTT_MS_LANG_KOREAN = 0x0412;
		public const int STBTT_MS_LANG_FRENCH = 0x040c;
		public const int STBTT_MS_LANG_RUSSIAN = 0x0419;
		public const int STBTT_MS_LANG_GERMAN = 0x0407;
		public const int STBTT_MS_LANG_SPANISH = 0x0409;
		public const int STBTT_MS_LANG_HEBREW = 0x040d;
		public const int STBTT_MS_LANG_SWEDISH = 0x041D;

		public const int STBTT_MAC_LANG_ENGLISH = 0;
		public const int STBTT_MAC_LANG_JAPANESE = 11;
		public const int STBTT_MAC_LANG_ARABIC = 12;
		public const int STBTT_MAC_LANG_KOREAN = 23;
		public const int STBTT_MAC_LANG_DUTCH = 4;
		public const int STBTT_MAC_LANG_RUSSIAN = 32;
		public const int STBTT_MAC_LANG_FRENCH = 1;
		public const int STBTT_MAC_LANG_SPANISH = 6;
		public const int STBTT_MAC_LANG_GERMAN = 2;
		public const int STBTT_MAC_LANG_SWEDISH = 5;
		public const int STBTT_MAC_LANG_HEBREW = 10;
		public const int STBTT_MAC_LANG_CHINESE_SIMPLIFIED = 33;
		public const int STBTT_MAC_LANG_ITALIAN = 3;
		public const int STBTT_MAC_LANG_CHINESE_TRAD = 19;

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt_bakedchar
		{
			public ushort x0;
			public ushort y0;
			public ushort x1;
			public ushort y1;
			public float xoff;
			public float yoff;
			public float xadvance;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt_aligned_quad
		{
			public float x0;
			public float y0;
			public float s0;
			public float t0;
			public float x1;
			public float y1;
			public float s1;
			public float t1;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt_kerningentry
		{
			public int glyph1;
			public int glyph2;
			public int advance;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt_vertex
		{
			public short x;
			public short y;
			public short cx;
			public short cy;
			public short cx1;
			public short cy1;
			public byte type;
			public byte padding;
		}

		public class stbtt__edge
		{
			public int invert;
			public float x0;
			public float x1;
			public float y0;
			public float y1;
		}

		public class stbtt__active_edge
		{
			public float direction;
			public float ey;
			public float fdx;
			public float fdy;
			public float fx;
			public stbtt__active_edge next;
			public float sy;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__point
		{
			public float x;
			public float y;
		}

		public static uint stbtt__find_table(FakePtr<byte> data, uint fontstart, string tag)
		{
			int num_tables = ttUSHORT(data + fontstart + 4);
			var tabledir = fontstart + 12;
			int i;
			for (i = 0; i < num_tables; ++i)
			{
				var loc = (uint)(tabledir + 16 * i);
				if ((data + loc + 0)[0] == tag[0] && (data + loc + 0)[1] == tag[1] &&
					(data + loc + 0)[2] == tag[2] && (data + loc + 0)[3] == tag[3])
					return ttULONG(data + loc + 8);
			}

			return 0;
		}

		public static ushort ttUSHORT(FakePtr<byte> p)
		{
			return (ushort)(p[0] * 256 + p[1]);
		}

		public static short ttSHORT(FakePtr<byte> p)
		{
			return (short)(p[0] * 256 + p[1]);
		}

		public static uint ttULONG(FakePtr<byte> p)
		{
			return (uint)((p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3]);
		}

		public static int ttLONG(FakePtr<byte> p)
		{
			return (p[0] << 24) + (p[1] << 16) + (p[2] << 8) + p[3];
		}

		public static int stbtt__isfont(FakePtr<byte> font)
		{
			if (font[0] == '1' && font[1] == 0 && font[2] == 0 && font[3] == 0)
				return 1;
			if (font[0] == "typ1"[0] && font[1] == "typ1"[1] && font[2] == "typ1"[2] && font[3] == "typ1"[3])
				return 1;
			if (font[0] == "OTTO"[0] && font[1] == "OTTO"[1] && font[2] == "OTTO"[2] && font[3] == "OTTO"[3])
				return 1;
			if (font[0] == 0 && font[1] == 1 && font[2] == 0 && font[3] == 0)
				return 1;
			if (font[0] == "true"[0] && font[1] == "true"[1] && font[2] == "true"[2] && font[3] == "true"[3])
				return 1;
			return 0;
		}

		public static int stbtt_GetFontOffsetForIndex_internal(FakePtr<byte> font_collection, int index)
		{
			if (stbtt__isfont(font_collection) != 0)
				return index == 0 ? 0 : -1;
			if (font_collection[0] == "ttcf"[0] && font_collection[1] == "ttcf"[1] && font_collection[2] == "ttcf"[2] &&
				font_collection[3] == "ttcf"[3])
				if (ttULONG(font_collection + 4) == 0x00010000 || ttULONG(font_collection + 4) == 0x00020000)
				{
					var n = ttLONG(font_collection + 8);
					if (index >= n)
						return -1;
					return (int)ttULONG(font_collection + 12 + index * 4);
				}

			return -1;
		}

		public static int stbtt_GetNumberOfFonts_internal(FakePtr<byte> font_collection)
		{
			if (stbtt__isfont(font_collection) != 0)
				return 1;
			if (font_collection[0] == "ttcf"[0] && font_collection[1] == "ttcf"[1] && font_collection[2] == "ttcf"[2] &&
				font_collection[3] == "ttcf"[3])
				if (ttULONG(font_collection + 4) == 0x00010000 || ttULONG(font_collection + 4) == 0x00020000)
					return ttLONG(font_collection + 8);

			return 0;
		}

		public static void stbtt_setvertex(ref stbtt_vertex v, byte type, int x, int y, int cx, int cy)
		{
			v.type = type;
			v.x = (short)x;
			v.y = (short)y;
			v.cx = (short)cx;
			v.cy = (short)cy;
		}

		public static int stbtt__close_shape(stbtt_vertex[] vertices, int num_vertices, int was_off, int start_off,
			int sx, int sy, int scx, int scy, int cx, int cy)
		{
			var v = new stbtt_vertex();
			if (start_off != 0)
			{
				if (was_off != 0)
				{
					stbtt_setvertex(ref v, STBTT_vcurve, (cx + scx) >> 1, (cy + scy) >> 1, cx, cy);
					vertices[num_vertices++] = v;
				}

				stbtt_setvertex(ref v, STBTT_vcurve, sx, sy, scx, scy);
				vertices[num_vertices++] = v;
			}
			else
			{
				if (was_off != 0)
				{
					stbtt_setvertex(ref v, STBTT_vcurve, sx, sy, cx, cy);
					vertices[num_vertices++] = v;
				}
				else
				{
					stbtt_setvertex(ref v, STBTT_vline, sx, sy, 0, 0);
					vertices[num_vertices++] = v;
				}
			}

			return num_vertices;
		}

		public static int stbtt__GetCoverageIndex(FakePtr<byte> coverageTable, int glyph)
		{
			var coverageFormat = ttUSHORT(coverageTable);
			switch (coverageFormat)
			{
				case 1:
					{
						var glyphCount = ttUSHORT(coverageTable + 2);
						var l = 0;
						var r = glyphCount - 1;
						var m = 0;
						var straw = 0;
						var needle = glyph;
						while (l <= r)
						{
							var glyphArray = coverageTable + 4;
							ushort glyphID = 0;
							m = (l + r) >> 1;
							glyphID = ttUSHORT(glyphArray + 2 * m);
							straw = glyphID;
							if (needle < straw)
								r = m - 1;
							else if (needle > straw)
								l = m + 1;
							else
								return m;
						}
					}
					break;
				case 2:
					{
						var rangeCount = ttUSHORT(coverageTable + 2);
						var rangeArray = coverageTable + 4;
						var l = 0;
						var r = rangeCount - 1;
						var m = 0;
						var strawStart = 0;
						var strawEnd = 0;
						var needle = glyph;
						while (l <= r)
						{
							FakePtr<byte> rangeRecord;
							m = (l + r) >> 1;
							rangeRecord = rangeArray + 6 * m;
							strawStart = ttUSHORT(rangeRecord);
							strawEnd = ttUSHORT(rangeRecord + 2);
							if (needle < strawStart)
							{
								r = m - 1;
							}
							else if (needle > strawEnd)
							{
								l = m + 1;
							}
							else
							{
								var startCoverageIndex = ttUSHORT(rangeRecord + 4);
								return startCoverageIndex + glyph - strawStart;
							}
						}
					}
					break;
				default:
					{
					}
					break;
			}

			return -1;
		}

		public static int stbtt__GetGlyphClass(FakePtr<byte> classDefTable, int glyph)
		{
			var classDefFormat = ttUSHORT(classDefTable);
			switch (classDefFormat)
			{
				case 1:
					{
						var startGlyphID = ttUSHORT(classDefTable + 2);
						var glyphCount = ttUSHORT(classDefTable + 4);
						var classDef1ValueArray = classDefTable + 6;
						if (glyph >= startGlyphID && glyph < startGlyphID + glyphCount)
							return ttUSHORT(classDef1ValueArray + 2 * (glyph - startGlyphID));
						classDefTable = classDef1ValueArray + 2 * glyphCount;
					}
					break;
				case 2:
					{
						var classRangeCount = ttUSHORT(classDefTable + 2);
						var classRangeRecords = classDefTable + 4;
						var l = 0;
						var r = classRangeCount - 1;
						var m = 0;
						var strawStart = 0;
						var strawEnd = 0;
						var needle = glyph;
						while (l <= r)
						{
							FakePtr<byte> classRangeRecord;
							m = (l + r) >> 1;
							classRangeRecord = classRangeRecords + 6 * m;
							strawStart = ttUSHORT(classRangeRecord);
							strawEnd = ttUSHORT(classRangeRecord + 2);
							if (needle < strawStart)
								r = m - 1;
							else if (needle > strawEnd)
								l = m + 1;
							else
								return ttUSHORT(classRangeRecord + 4);
						}

						classDefTable = classRangeRecords + 6 * classRangeCount;
					}
					break;
				default:
					{
					}
					break;
			}

			return -1;
		}

		public static stbtt__active_edge stbtt__new_active(stbtt__edge e, int off_x, float start_point)
		{
			var z = new stbtt__active_edge();
			var dxdy = (e.x1 - e.x0) / (e.y1 - e.y0);
			z.fdx = dxdy;
			z.fdy = dxdy != 0.0f ? 1.0f / dxdy : 0.0f;
			z.fx = e.x0 + dxdy * (start_point - e.y0);
			z.fx -= off_x;
			z.direction = e.invert != 0 ? 1.0f : -1.0f;
			z.sy = e.y0;
			z.ey = e.y1;
			z.next = null;
			return z;
		}

		public static void stbtt__sort_edges_ins_sort(FakePtr<stbtt__edge> p, int n)
		{
			var i = 0;
			var j = 0;
			for (i = 1; i < n; ++i)
			{
				var t = p[i];
				var a = t;
				j = i;
				while (j > 0)
				{
					var b = p[j - 1];
					var c = a.y0 < b.y0 ? 1 : 0;
					if (c == 0)
						break;
					p[j] = p[j - 1];
					--j;
				}

				if (i != j)
					p[j] = t;
			}
		}

		public static void stbtt__sort_edges_quicksort(FakePtr<stbtt__edge> p, int n)
		{
			while (n > 12)
			{
				var t = new stbtt__edge();
				var c01 = 0;
				var c12 = 0;
				var c = 0;
				var m = 0;
				var i = 0;
				var j = 0;
				m = n >> 1;
				c01 = p[0].y0 < p[m].y0 ? 1 : 0;
				c12 = p[m].y0 < p[n - 1].y0 ? 1 : 0;
				if (c01 != c12)
				{
					var z = 0;
					c = p[0].y0 < p[n - 1].y0 ? 1 : 0;
					z = c == c12 ? 0 : n - 1;
					t = p[z];
					p[z] = p[m];
					p[m] = t;
				}

				t = p[0];
				p[0] = p[m];
				p[m] = t;
				i = 1;
				j = n - 1;
				for (; ; )
				{
					for (; ; ++i)
						if (!(p[i].y0 < p[0].y0))
							break;
					for (; ; --j)
						if (!(p[0].y0 < p[j].y0))
							break;
					if (i >= j)
						break;
					t = p[i];
					p[i] = p[j];
					p[j] = t;
					++i;
					--j;
				}

				if (j < n - i)
				{
					stbtt__sort_edges_quicksort(p, j);
					p = p + i;
					n = n - i;
				}
				else
				{
					stbtt__sort_edges_quicksort(p + i, n - i);
					n = j;
				}
			}
		}

		public static void stbtt__sort_edges(FakePtr<stbtt__edge> p, int n)
		{
			stbtt__sort_edges_quicksort(p, n);
			stbtt__sort_edges_ins_sort(p, n);
		}

		public static void stbtt__add_point(stbtt__point[] points, int n, float x, float y)
		{
			if (points == null)
				return;
			points[n].x = x;
			points[n].y = y;
		}

		public static int stbtt__tesselate_curve(stbtt__point[] points, ref int num_points, float x0, float y0,
			float x1, float y1, float x2, float y2, float objspace_flatness_squared, int n)
		{
			var mx = (x0 + 2 * x1 + x2) / 4;
			var my = (y0 + 2 * y1 + y2) / 4;
			var dx = (x0 + x2) / 2 - mx;
			var dy = (y0 + y2) / 2 - my;
			if (n > 16)
				return 1;
			if (dx * dx + dy * dy > objspace_flatness_squared)
			{
				stbtt__tesselate_curve(points, ref num_points, x0, y0, (x0 + x1) / 2.0f, (y0 + y1) / 2.0f, mx, my,
					objspace_flatness_squared, n + 1);
				stbtt__tesselate_curve(points, ref num_points, mx, my, (x1 + x2) / 2.0f, (y1 + y2) / 2.0f, x2, y2,
					objspace_flatness_squared, n + 1);
			}
			else
			{
				stbtt__add_point(points, num_points, x2, y2);
				num_points++;
			}

			return 1;
		}

		public static void stbtt__tesselate_cubic(stbtt__point[] points, ref int num_points, float x0, float y0,
			float x1, float y1, float x2, float y2, float x3, float y3, float objspace_flatness_squared, int n)
		{
			var dx0 = x1 - x0;
			var dy0 = y1 - y0;
			var dx1 = x2 - x1;
			var dy1 = y2 - y1;
			var dx2 = x3 - x2;
			var dy2 = y3 - y2;
			var dx = x3 - x0;
			var dy = y3 - y0;
			var longlen = (float)(Math.Sqrt(dx0 * dx0 + dy0 * dy0) + Math.Sqrt(dx1 * dx1 + dy1 * dy1) +
								   Math.Sqrt(dx2 * dx2 + dy2 * dy2));
			var shortlen = (float)Math.Sqrt(dx * dx + dy * dy);
			var flatness_squared = longlen * longlen - shortlen * shortlen;
			if (n > 16)
				return;
			if (flatness_squared > objspace_flatness_squared)
			{
				var x01 = (x0 + x1) / 2;
				var y01 = (y0 + y1) / 2;
				var x12 = (x1 + x2) / 2;
				var y12 = (y1 + y2) / 2;
				var x23 = (x2 + x3) / 2;
				var y23 = (y2 + y3) / 2;
				var xa = (x01 + x12) / 2;
				var ya = (y01 + y12) / 2;
				var xb = (x12 + x23) / 2;
				var yb = (y12 + y23) / 2;
				var mx = (xa + xb) / 2;
				var my = (ya + yb) / 2;
				stbtt__tesselate_cubic(points, ref num_points, x0, y0, x01, y01, xa, ya, mx, my,
					objspace_flatness_squared, n + 1);
				stbtt__tesselate_cubic(points, ref num_points, mx, my, xb, yb, x23, y23, x3, y3,
					objspace_flatness_squared, n + 1);
			}
			else
			{
				stbtt__add_point(points, num_points, x3, y3);
				num_points++;
			}
		}

		public static stbtt__point[] stbtt_FlattenCurves(stbtt_vertex[] vertices, int num_verts,
			float objspace_flatness, out int[] contour_lengths, out int num_contours)
		{
			stbtt__point[] points = null;
			var num_points = 0;
			var objspace_flatness_squared = objspace_flatness * objspace_flatness;
			var i = 0;
			var n = 0;
			var start = 0;
			var pass = 0;
			for (i = 0; i < num_verts; ++i)
				if (vertices[i].type == STBTT_vmove)
					++n;
			num_contours = n;
			contour_lengths = null;
			if (n == 0)
				return null;
			contour_lengths = new int[n];

			for (pass = 0; pass < 2; ++pass)
			{
				var x = (float)0;
				var y = (float)0;
				if (pass == 1)
					points = new stbtt__point[num_points];
				num_points = 0;
				n = -1;
				for (i = 0; i < num_verts; ++i)
					switch (vertices[i].type)
					{
						case STBTT_vmove:
							if (n >= 0)
								contour_lengths[n] = num_points - start;
							++n;
							start = num_points;
							x = vertices[i].x;
							y = vertices[i].y;
							stbtt__add_point(points, num_points++, x, y);
							break;
						case STBTT_vline:
							x = vertices[i].x;
							y = vertices[i].y;
							stbtt__add_point(points, num_points++, x, y);
							break;
						case STBTT_vcurve:
							stbtt__tesselate_curve(points, ref num_points, x, y, vertices[i].cx, vertices[i].cy,
								vertices[i].x, vertices[i].y, objspace_flatness_squared, 0);
							x = vertices[i].x;
							y = vertices[i].y;
							break;
						case STBTT_vcubic:
							stbtt__tesselate_cubic(points, ref num_points, x, y, vertices[i].cx, vertices[i].cy,
								vertices[i].cx1, vertices[i].cy1, vertices[i].x, vertices[i].y,
								objspace_flatness_squared, 0);
							x = vertices[i].x;
							y = vertices[i].y;
							break;
					}

				contour_lengths[n] = num_points - start;
			}

			return points;
		}

		public static int stbtt_BakeFontBitmap_internal(byte[] data, int offset, float pixel_height,
			FakePtr<byte> pixels, int pw, int ph, int first_char, int num_chars, stbtt_bakedchar[] chardata)
		{
			float scale = 0;
			var x = 0;
			var y = 0;
			var bottom_y = 0;
			var i = 0;
			var f = new FontInfo();
			if (f.stbtt_InitFont(data, offset) == 0)
				return -1;
			pixels.memset(0, pw * ph);
			x = y = 1;
			bottom_y = 1;
			scale = f.stbtt_ScaleForPixelHeight(pixel_height);
			for (i = 0; i < num_chars; ++i)
			{
				var advance = 0;
				var lsb = 0;
				var x0 = 0;
				var y0 = 0;
				var x1 = 0;
				var y1 = 0;
				var gw = 0;
				var gh = 0;
				var g = f.stbtt_FindGlyphIndex(first_char + i);
				f.stbtt_GetGlyphHMetrics(g, ref advance, ref lsb);
				f.stbtt_GetGlyphBitmapBox(g, scale, scale, ref x0, ref y0, ref x1, ref y1);
				gw = x1 - x0;
				gh = y1 - y0;
				if (x + gw + 1 >= pw)
				{
					y = bottom_y;
					x = 1;
				}

				if (y + gh + 1 >= ph)
					return -i;
				f.stbtt_MakeGlyphBitmap(pixels + x + y * pw, gw, gh, pw, scale, scale, g);
				chardata[i].x0 = (ushort)(short)x;
				chardata[i].y0 = (ushort)(short)y;
				chardata[i].x1 = (ushort)(short)(x + gw);
				chardata[i].y1 = (ushort)(short)(y + gh);
				chardata[i].xadvance = scale * advance;
				chardata[i].xoff = x0;
				chardata[i].yoff = y0;
				x = x + gw + 1;
				if (y + gh + 1 > bottom_y)
					bottom_y = y + gh + 1;
			}

			return bottom_y;
		}

		public static void stbtt_GetBakedQuad(stbtt_bakedchar[] chardata, int pw, int ph, int char_index,
			ref float xpos, ref float ypos, ref stbtt_aligned_quad q, int opengl_fillrule)
		{
			var d3d_bias = opengl_fillrule != 0 ? 0 : -0.5f;
			var ipw = 1.0f / pw;
			var iph = 1.0f / ph;
			var round_x = (int)Math.Floor(xpos + chardata[char_index].xoff + 0.5f);
			var round_y = (int)Math.Floor(ypos + chardata[char_index].yoff + 0.5f);
			q.x0 = round_x + d3d_bias;
			q.y0 = round_y + d3d_bias;
			q.x1 = round_x + chardata[char_index].x1 - chardata[char_index].x0 + d3d_bias;
			q.y1 = round_y + chardata[char_index].y1 - chardata[char_index].y0 + d3d_bias;
			q.s0 = chardata[char_index].x0 * ipw;
			q.t0 = chardata[char_index].y0 * iph;
			q.s1 = chardata[char_index].x1 * ipw;
			q.t1 = chardata[char_index].y1 * iph;
			xpos += chardata[char_index].xadvance;
		}

		public static void stbtt__h_prefilter(FakePtr<byte> pixels, int w, int h, int stride_in_bytes,
			uint kernel_width)
		{
			var buffer = new byte[8];
			var safe_w = (int)(w - kernel_width);
			var j = 0;

			Array.Clear(buffer, 0, 8);
			for (j = 0; j < h; ++j)
			{
				var i = 0;
				uint total = 0;
				Array.Clear(buffer, 0, (int)kernel_width);
				total = 0;
				switch (kernel_width)
				{
					case 2:
						for (i = 0; i <= safe_w; ++i)
						{
							total += (uint)(pixels[i] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i];
							pixels[i] = (byte)(total / 2);
						}

						break;
					case 3:
						for (i = 0; i <= safe_w; ++i)
						{
							total += (uint)(pixels[i] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i];
							pixels[i] = (byte)(total / 3);
						}

						break;
					case 4:
						for (i = 0; i <= safe_w; ++i)
						{
							total += (uint)(pixels[i] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i];
							pixels[i] = (byte)(total / 4);
						}

						break;
					case 5:
						for (i = 0; i <= safe_w; ++i)
						{
							total += (uint)(pixels[i] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i];
							pixels[i] = (byte)(total / 5);
						}

						break;
					default:
						for (i = 0; i <= safe_w; ++i)
						{
							total += (uint)(pixels[i] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i];
							pixels[i] = (byte)(total / kernel_width);
						}

						break;
				}

				for (; i < w; ++i)
				{
					total -= buffer[i & (8 - 1)];
					pixels[i] = (byte)(total / kernel_width);
				}

				pixels += stride_in_bytes;
			}
		}

		public static void stbtt__v_prefilter(FakePtr<byte> pixels, int w, int h, int stride_in_bytes,
			uint kernel_width)
		{
			var buffer = new byte[8];
			var safe_h = (int)(h - kernel_width);
			var j = 0;
			Array.Clear(buffer, 0, 8);
			for (j = 0; j < w; ++j)
			{
				var i = 0;
				uint total = 0;
				Array.Clear(buffer, 0, (int)kernel_width);
				total = 0;
				switch (kernel_width)
				{
					case 2:
						for (i = 0; i <= safe_h; ++i)
						{
							total += (uint)(pixels[i * stride_in_bytes] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i * stride_in_bytes];
							pixels[i * stride_in_bytes] = (byte)(total / 2);
						}

						break;
					case 3:
						for (i = 0; i <= safe_h; ++i)
						{
							total += (uint)(pixels[i * stride_in_bytes] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i * stride_in_bytes];
							pixels[i * stride_in_bytes] = (byte)(total / 3);
						}

						break;
					case 4:
						for (i = 0; i <= safe_h; ++i)
						{
							total += (uint)(pixels[i * stride_in_bytes] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i * stride_in_bytes];
							pixels[i * stride_in_bytes] = (byte)(total / 4);
						}

						break;
					case 5:
						for (i = 0; i <= safe_h; ++i)
						{
							total += (uint)(pixels[i * stride_in_bytes] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i * stride_in_bytes];
							pixels[i * stride_in_bytes] = (byte)(total / 5);
						}

						break;
					default:
						for (i = 0; i <= safe_h; ++i)
						{
							total += (uint)(pixels[i * stride_in_bytes] - buffer[i & (8 - 1)]);
							buffer[(i + kernel_width) & (8 - 1)] = pixels[i * stride_in_bytes];
							pixels[i * stride_in_bytes] = (byte)(total / kernel_width);
						}

						break;
				}

				for (; i < h; ++i)
				{
					total -= buffer[i & (8 - 1)];
					pixels[i * stride_in_bytes] = (byte)(total / kernel_width);
				}

				pixels += 1;
			}
		}

		public static float stbtt__oversample_shift(int oversample)
		{
			if (oversample == 0)
				return 0.0f;
			return -(oversample - 1) / (2.0f * oversample);
		}

		public static void stbtt_GetScaledFontVMetrics(byte[] fontdata, int index, float size, ref float ascent,
			ref float descent, ref float lineGap)
		{
			var i_ascent = 0;
			var i_descent = 0;
			var i_lineGap = 0;
			float scale = 0;
			var info = new FontInfo();
			info.stbtt_InitFont(fontdata, stbtt_GetFontOffsetForIndex(fontdata, index));
			scale = size > 0 ? info.stbtt_ScaleForPixelHeight(size) : info.stbtt_ScaleForMappingEmToPixels(-size);
			info.stbtt_GetFontVMetrics(out i_ascent, out i_descent, out i_lineGap);
			ascent = i_ascent * scale;
			descent = i_descent * scale;
			lineGap = i_lineGap * scale;
		}

		public static void stbtt_GetPackedQuad(stbtt_packedchar[] chardata, int pw, int ph, int char_index,
			ref float xpos, ref float ypos, ref stbtt_aligned_quad q, int align_to_integer)
		{
			var ipw = 1.0f / pw;
			var iph = 1.0f / ph;
			var b = chardata[char_index];
			if (align_to_integer != 0)
			{
				var x = (float)(int)Math.Floor(xpos + b.xoff + 0.5f);
				var y = (float)(int)Math.Floor(ypos + b.yoff + 0.5f);
				q.x0 = x;
				q.y0 = y;
				q.x1 = x + b.xoff2 - b.xoff;
				q.y1 = y + b.yoff2 - b.yoff;
			}
			else
			{
				q.x0 = xpos + b.xoff;
				q.y0 = ypos + b.yoff;
				q.x1 = xpos + b.xoff2;
				q.y1 = ypos + b.yoff2;
			}

			q.s0 = b.x0 * ipw;
			q.t0 = b.y0 * iph;
			q.s1 = b.x1 * ipw;
			q.t1 = b.y1 * iph;
			xpos += b.xadvance;
		}

		public static int stbtt__ray_intersect_bezier(float[] orig, float[] ray, float[] q0, float[] q1, float[] q2,
			float[] hits)
		{
			var q0perp = q0[1] * ray[0] - q0[0] * ray[1];
			var q1perp = q1[1] * ray[0] - q1[0] * ray[1];
			var q2perp = q2[1] * ray[0] - q2[0] * ray[1];
			var roperp = orig[1] * ray[0] - orig[0] * ray[1];
			var a = q0perp - 2 * q1perp + q2perp;
			var b = q1perp - q0perp;
			var c = q0perp - roperp;
			var s0 = (float)0;
			var s1 = (float)0;
			var num_s = 0;
			if (a != 0.0)
			{
				var discr = b * b - a * c;
				if (discr > 0.0)
				{
					var rcpna = -1 / a;
					var d = (float)Math.Sqrt(discr);
					s0 = (b + d) * rcpna;
					s1 = (b - d) * rcpna;
					if (s0 >= 0.0 && s0 <= 1.0)
						num_s = 1;
					if (d > 0.0 && s1 >= 0.0 && s1 <= 1.0)
					{
						if (num_s == 0)
							s0 = s1;
						++num_s;
					}
				}
			}
			else
			{
				s0 = c / (-2 * b);
				if (s0 >= 0.0 && s0 <= 1.0)
					num_s = 1;
			}

			if (num_s == 0)
				return 0;

			var rcp_len2 = 1 / (ray[0] * ray[0] + ray[1] * ray[1]);
			var rayn_x = ray[0] * rcp_len2;
			var rayn_y = ray[1] * rcp_len2;
			var q0d = q0[0] * rayn_x + q0[1] * rayn_y;
			var q1d = q1[0] * rayn_x + q1[1] * rayn_y;
			var q2d = q2[0] * rayn_x + q2[1] * rayn_y;
			var rod = orig[0] * rayn_x + orig[1] * rayn_y;
			var q10d = q1d - q0d;
			var q20d = q2d - q0d;
			var q0rd = q0d - rod;
			hits[0] = q0rd + s0 * (2.0f - 2.0f * s0) * q10d + s0 * s0 * q20d;
			hits[1] = a * s0 + b;
			if (num_s > 1)
			{
				hits[2] = q0rd + s1 * (2.0f - 2.0f * s1) * q10d + s1 * s1 * q20d;
				hits[3] = a * s1 + b;
				return 2;
			}

			return 1;
		}

		public static int equal(float[] a, float[] b)
		{
			return a[0] == b[0] && a[1] == b[1] ? 1 : 0;
		}

		public static int stbtt__compute_crossings_x(float x, float y, int nverts, stbtt_vertex[] verts)
		{
			var i = 0;
			var orig = new float[2];
			var ray = new float[2];
			ray[0] = 1;
			ray[1] = 0;

			float y_frac = 0;
			var winding = 0;
			orig[0] = x;
			orig[1] = y;
			y_frac = y % 1.0f;
			if (y_frac < 0.01f)
				y += 0.01f;
			else if (y_frac > 0.99f)
				y -= 0.01f;
			orig[1] = y;
			for (i = 0; i < nverts; ++i)
			{
				if (verts[i].type == STBTT_vline)
				{
					var x0 = (int)verts[i - 1].x;
					var y0 = (int)verts[i - 1].y;
					var x1 = (int)verts[i].x;
					var y1 = (int)verts[i].y;
					if (y > (y0 < y1 ? y0 : y1) && y < (y0 < y1 ? y1 : y0) && x > (x0 < x1 ? x0 : x1))
					{
						var x_inter = (y - y0) / (y1 - y0) * (x1 - x0) + x0;
						if (x_inter < x)
							winding += y0 < y1 ? 1 : -1;
					}
				}

				if (verts[i].type == STBTT_vcurve)
				{
					var x0 = (int)verts[i - 1].x;
					var y0 = (int)verts[i - 1].y;
					var x1 = (int)verts[i].cx;
					var y1 = (int)verts[i].cy;
					var x2 = (int)verts[i].x;
					var y2 = (int)verts[i].y;
					var ax = x0 < (x1 < x2 ? x1 : x2) ? x0 : x1 < x2 ? x1 : x2;
					var ay = y0 < (y1 < y2 ? y1 : y2) ? y0 : y1 < y2 ? y1 : y2;
					var by = y0 < (y1 < y2 ? y2 : y1) ? y1 < y2 ? y2 : y1 : y0;
					if (y > ay && y < by && x > ax)
					{
						var q0 = new float[2];
						var q1 = new float[2];
						var q2 = new float[2];
						var hits = new float[4];
						q0[0] = x0;
						q0[1] = y0;
						q1[0] = x1;
						q1[1] = y1;
						q2[0] = x2;
						q2[1] = y2;
						if (equal(q0, q1) != 0 || equal(q1, q2) != 0)
						{
							x0 = verts[i - 1].x;
							y0 = verts[i - 1].y;
							x1 = verts[i].x;
							y1 = verts[i].y;
							if (y > (y0 < y1 ? y0 : y1) && y < (y0 < y1 ? y1 : y0) && x > (x0 < x1 ? x0 : x1))
							{
								var x_inter = (y - y0) / (y1 - y0) * (x1 - x0) + x0;
								if (x_inter < x)
									winding += y0 < y1 ? 1 : -1;
							}
						}
						else
						{
							var num_hits = stbtt__ray_intersect_bezier(orig, ray, q0, q1, q2, hits);
							if (num_hits >= 1)
								if (hits[0] < 0)
									winding += hits[1] < 0 ? -1 : 1;
							if (num_hits >= 2)
								if (hits[2] < 0)
									winding += hits[3] < 0 ? -1 : 1;
						}
					}
				}
			}

			return winding;
		}

		public static float stbtt__cuberoot(float x)
		{
			if (x < 0)
				return -(float)Math.Pow(-x, 1.0f / 3.0f);
			return (float)Math.Pow(x, 1.0f / 3.0f);
		}

		public static int stbtt__solve_cubic(float a, float b, float c, float[] r)
		{
			var s = -a / 3;
			var p = b - a * a / 3;
			var q = a * (2 * a * a - 9 * b) / 27 + c;
			var p3 = p * p * p;
			var d = q * q + 4 * p3 / 27;
			if (d >= 0)
			{
				var z = (float)Math.Sqrt(d);
				var u = (-q + z) / 2;
				var v = (-q - z) / 2;
				u = stbtt__cuberoot(u);
				v = stbtt__cuberoot(v);
				r[0] = s + u + v;
				return 1;
			}
			else
			{
				var u = (float)Math.Sqrt(-p / 3);
				var v = (float)Math.Acos(-Math.Sqrt(-27 / p3) * q / 2) / 3;
				var m = (float)Math.Cos(v);
				var n = (float)Math.Cos(v - 3.141592 / 2) * 1.732050808f;
				r[0] = s + u * 2 * m;
				r[1] = s - u * (m + n);
				r[2] = s - u * (m - n);
				return 3;
			}
		}

		public static int stbtt__CompareUTF8toUTF16_bigendian_prefix(FakePtr<byte> s1, int len1, FakePtr<byte> s2,
			int len2)
		{
			var i = 0;
			while (len2 != 0)
			{
				var ch = (ushort)(s2[0] * 256 + s2[1]);
				if (ch < 0x80)
				{
					if (i >= len1)
						return -1;
					if (s1[i++] != ch)
						return -1;
				}
				else if (ch < 0x800)
				{
					if (i + 1 >= len1)
						return -1;
					if (s1[i++] != 0xc0 + (ch >> 6))
						return -1;
					if (s1[i++] != 0x80 + (ch & 0x3f))
						return -1;
				}
				else if (ch >= 0xd800 && ch < 0xdc00)
				{
					uint c = 0;
					var ch2 = (ushort)(s2[2] * 256 + s2[3]);
					if (i + 3 >= len1)
						return -1;
					c = (uint)(((ch - 0xd800) << 10) + (ch2 - 0xdc00) + 0x10000);
					if (s1[i++] != 0xf0 + (c >> 18))
						return -1;
					if (s1[i++] != 0x80 + ((c >> 12) & 0x3f))
						return -1;
					if (s1[i++] != 0x80 + ((c >> 6) & 0x3f))
						return -1;
					if (s1[i++] != 0x80 + (c & 0x3f))
						return -1;
					s2 += 2;
					len2 -= 2;
				}
				else if (ch >= 0xdc00 && ch < 0xe000)
				{
					return -1;
				}
				else
				{
					if (i + 2 >= len1)
						return -1;
					if (s1[i++] != 0xe0 + (ch >> 12))
						return -1;
					if (s1[i++] != 0x80 + ((ch >> 6) & 0x3f))
						return -1;
					if (s1[i++] != 0x80 + (ch & 0x3f))
						return -1;
				}

				s2 += 2;
				len2 -= 2;
			}

			return i;
		}

		public static int stbtt_CompareUTF8toUTF16_bigendian_internal(FakePtr<byte> s1, int len1, FakePtr<byte> s2,
			int len2)
		{
			return len1 == stbtt__CompareUTF8toUTF16_bigendian_prefix(s1, len1, s2, len2) ? 1 : 0;
		}

		public static int stbtt__matchpair(FakePtr<byte> fc, uint nm, FakePtr<byte> name, int nlen, int target_id,
			int next_id)
		{
			var i = 0;
			var count = (int)ttUSHORT(fc + nm + 2);
			var stringOffset = (int)(nm + ttUSHORT(fc + nm + 4));
			for (i = 0; i < count; ++i)
			{
				var loc = (uint)(nm + 6 + 12 * i);
				var id = (int)ttUSHORT(fc + loc + 6);
				if (id == target_id)
				{
					var platform = (int)ttUSHORT(fc + loc + 0);
					var encoding = (int)ttUSHORT(fc + loc + 2);
					var language = (int)ttUSHORT(fc + loc + 4);
					if (platform == 0 || platform == 3 && encoding == 1 || platform == 3 && encoding == 10)
					{
						var slen = (int)ttUSHORT(fc + loc + 8);
						var off = (int)ttUSHORT(fc + loc + 10);
						var matchlen =
							stbtt__CompareUTF8toUTF16_bigendian_prefix(name, nlen, fc + stringOffset + off, slen);
						if (matchlen >= 0)
						{
							if (i + 1 < count && ttUSHORT(fc + loc + 12 + 6) == next_id &&
								ttUSHORT(fc + loc + 12) == platform && ttUSHORT(fc + loc + 12 + 2) == encoding &&
								ttUSHORT(fc + loc + 12 + 4) == language)
							{
								slen = ttUSHORT(fc + loc + 12 + 8);
								off = ttUSHORT(fc + loc + 12 + 10);
								if (slen == 0)
								{
									if (matchlen == nlen)
										return 1;
								}
								else if (matchlen < nlen && name[matchlen] == ' ')
								{
									++matchlen;
									if (stbtt_CompareUTF8toUTF16_bigendian_internal(name + matchlen, nlen - matchlen,
											fc + stringOffset + off, slen) != 0)
										return 1;
								}
							}
							else
							{
								if (matchlen == nlen)
									return 1;
							}
						}
					}
				}
			}

			return 0;
		}

		public static int stbtt__matches(byte[] data, uint offset, FakePtr<byte> name, int flags)
		{
			var nlen = 0;
			var ptr = name;

			while (ptr.GetAndIncrease() != '\0')
				ptr++;

			nlen = ptr.Offset - name.Offset - 1;
			uint nm = 0;
			uint hd = 0;

			var fc = new FakePtr<byte>(data);
			if (stbtt__isfont(fc + offset) == 0)
				return 0;
			if (flags != 0)
			{
				hd = stbtt__find_table(fc, offset, "head");
				if ((ttUSHORT(fc + hd + 44) & 7) != (flags & 7))
					return 0;
			}

			nm = stbtt__find_table(fc, offset, "name");
			if (nm == 0)
				return 0;
			if (flags != 0)
			{
				if (stbtt__matchpair(fc, nm, name, nlen, 16, -1) != 0)
					return 1;
				if (stbtt__matchpair(fc, nm, name, nlen, 1, -1) != 0)
					return 1;
				if (stbtt__matchpair(fc, nm, name, nlen, 3, -1) != 0)
					return 1;
			}
			else
			{
				if (stbtt__matchpair(fc, nm, name, nlen, 16, 17) != 0)
					return 1;
				if (stbtt__matchpair(fc, nm, name, nlen, 1, 2) != 0)
					return 1;
				if (stbtt__matchpair(fc, nm, name, nlen, 3, -1) != 0)
					return 1;
			}

			return 0;
		}

		public static int stbtt_FindMatchingFont_internal(byte[] font_collection, FakePtr<byte> name_utf8, int flags)
		{
			var i = 0;
			for (i = 0; ; ++i)
			{
				var off = stbtt_GetFontOffsetForIndex(font_collection, i);
				if (off < 0)
					return off;
				if (stbtt__matches(font_collection, (uint)off, name_utf8, flags) != 0)
					return off;
			}
		}

		public static int stbtt_BakeFontBitmap(byte[] data, int offset, float pixel_height, FakePtr<byte> pixels,
			int pw, int ph, int first_char, int num_chars, stbtt_bakedchar[] chardata)
		{
			return stbtt_BakeFontBitmap_internal(data, offset, pixel_height, pixels, pw, ph, first_char, num_chars,
				chardata);
		}

		public static int stbtt_GetFontOffsetForIndex(byte[] data, int index)
		{
			return stbtt_GetFontOffsetForIndex_internal(new FakePtr<byte>(data), index);
		}

		public static int stbtt_GetNumberOfFonts(FakePtr<byte> data)
		{
			return stbtt_GetNumberOfFonts_internal(data);
		}

		public static int stbtt_FindMatchingFont(byte[] fontdata, FakePtr<byte> name, int flags)
		{
			return stbtt_FindMatchingFont_internal(fontdata, name, flags);
		}

		public static int stbtt_CompareUTF8toUTF16_bigendian(FakePtr<byte> s1, int len1, FakePtr<byte> s2, int len2)
		{
			return stbtt_CompareUTF8toUTF16_bigendian_internal(s1, len1, s2, len2);
		}
	}
}