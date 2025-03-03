using System;

namespace StbTrueTypeSharp
{
	public class PackContext
	{
		public uint h_oversample;
		public int height;
		public stbrp_context pack_info;
		public int padding;
		public FakePtr<byte> pixels;
		public int skip_missing;
		public int stride_in_bytes;
		public uint v_oversample;
		public int width;

		public class stbrp_context
		{
			public int bottom_y;
			public int height;
			public int width;
			public int x;
			public int y;

			public stbrp_context(int pw, int ph)
			{
				width = pw;
				height = ph;
				x = 0;
				y = 0;
				bottom_y = 0;
			}

			public void stbrp_pack_rects(stbrp_rect[] rects, int num_rects)
			{
				var i = 0;
				for (i = 0; i < num_rects; ++i)
				{
					if (this.x + rects[i].w > this.width)
					{
						this.x = 0;
						this.y = this.bottom_y;
					}

					if (this.y + rects[i].h > this.height)
						break;
					rects[i].x = this.x;
					rects[i].y = this.y;
					rects[i].was_packed = 1;
					this.x += rects[i].w;
					if (this.y + rects[i].h > this.bottom_y)
						this.bottom_y = this.y + rects[i].h;
				}

				for (; i < num_rects; ++i)
					rects[i].was_packed = 0;
			}
		}

		public class stbrp_rect
		{
			public int h;
			public int id;
			public int w;
			public int was_packed;
			public int x;
			public int y;
		}

		public class stbtt_packedchar
		{
			public ushort x0;
			public ushort x1;
			public float xadvance;
			public float xoff;
			public float xoff2;
			public ushort y0;
			public ushort y1;
			public float yoff;
			public float yoff2;
		}

		public class stbtt_pack_range
		{
			public int[] array_of_unicode_codepoints;
			public stbtt_packedchar[] chardata_for_range;
			public int first_unicode_codepoint_in_range;
			public float font_size;
			public byte h_oversample;
			public int num_chars;
			public byte v_oversample;
		}

		public int stbtt_PackBegin(byte[] pixels, int pw, int ph, int stride_in_bytes, int padding)
		{
			var context = new stbrp_context(pw - padding, ph - padding);

			this.width = pw;
			this.height = ph;
			this.pixels = new FakePtr<byte>(pixels);
			this.pack_info = context;
			this.padding = padding;
			this.stride_in_bytes = stride_in_bytes != 0 ? stride_in_bytes : pw;
			this.h_oversample = 1;
			this.v_oversample = 1;
			this.skip_missing = 0;
			if (pixels != null)
				Array.Clear(pixels, 0, pw * ph);
			return 1;
		}

		public void stbtt_PackSetOversampling(uint h_oversample, uint v_oversample)
		{
			if (h_oversample <= 8)
				this.h_oversample = h_oversample;
			if (v_oversample <= 8)
				this.v_oversample = v_oversample;
		}

		public void stbtt_PackSetSkipMissingCodepoints(int skip)
		{
			this.skip_missing = skip;
		}

		public int stbtt_PackFontRangesGatherRects(FontInfo info,
			FakePtr<stbtt_pack_range> ranges, int num_ranges, stbrp_rect[] rects)
		{
			var i = 0;
			var j = 0;
			var k = 0;
			var missing_glyph_added = 0;
			k = 0;
			for (i = 0; i < num_ranges; ++i)
			{
				var fh = ranges[i].font_size;
				var scale = fh > 0 ? info.stbtt_ScaleForPixelHeight(fh) : info.stbtt_ScaleForMappingEmToPixels(-fh);
				ranges[i].h_oversample = (byte)this.h_oversample;
				ranges[i].v_oversample = (byte)this.v_oversample;
				for (j = 0; j < ranges[i].num_chars; ++j)
				{
					var x0 = 0;
					var y0 = 0;
					var x1 = 0;
					var y1 = 0;
					var codepoint = ranges[i].array_of_unicode_codepoints == null
						? ranges[i].first_unicode_codepoint_in_range + j
						: ranges[i].array_of_unicode_codepoints[j];
					var glyph = info.stbtt_FindGlyphIndex(codepoint);
					if (glyph == 0 && (this.skip_missing != 0 || missing_glyph_added != 0))
					{
						rects[k].w = rects[k].h = 0;
					}
					else
					{
						info.stbtt_GetGlyphBitmapBoxSubpixel(glyph, scale * this.h_oversample, scale * this.v_oversample,
							0, 0, ref x0, ref y0, ref x1, ref y1);
						rects[k].w = (int)(x1 - x0 + this.padding + this.h_oversample - 1);
						rects[k].h = (int)(y1 - y0 + this.padding + this.v_oversample - 1);
						if (glyph == 0)
							missing_glyph_added = 1;
					}

					++k;
				}
			}

			return k;
		}

		public int stbtt_PackFontRangesRenderIntoRects(FontInfo info,
			FakePtr<stbtt_pack_range> ranges, int num_ranges, stbrp_rect[] rects)
		{
			var i = 0;
			var j = 0;
			var k = 0;
			var missing_glyph = -1;
			var return_value = 1;
			var old_h_over = (int)this.h_oversample;
			var old_v_over = (int)this.v_oversample;
			k = 0;
			for (i = 0; i < num_ranges; ++i)
			{
				var fh = ranges[i].font_size;
				var scale = fh > 0 ? info.stbtt_ScaleForPixelHeight(fh) : info.stbtt_ScaleForMappingEmToPixels(-fh);
				float recip_h = 0;
				float recip_v = 0;
				float sub_x = 0;
				float sub_y = 0;
				this.h_oversample = ranges[i].h_oversample;
				this.v_oversample = ranges[i].v_oversample;
				recip_h = 1.0f / this.h_oversample;
				recip_v = 1.0f / this.v_oversample;
				sub_x = Common.stbtt__oversample_shift((int)this.h_oversample);
				sub_y = Common.stbtt__oversample_shift((int)this.v_oversample);
				for (j = 0; j < ranges[i].num_chars; ++j)
				{
					var r = rects[k];
					if (r.was_packed != 0 && r.w != 0 && r.h != 0)
					{
						var bc = ranges[i].chardata_for_range[j];
						var advance = 0;
						var lsb = 0;
						var x0 = 0;
						var y0 = 0;
						var x1 = 0;
						var y1 = 0;
						var codepoint = ranges[i].array_of_unicode_codepoints == null
							? ranges[i].first_unicode_codepoint_in_range + j
							: ranges[i].array_of_unicode_codepoints[j];
						var glyph = info.stbtt_FindGlyphIndex(codepoint);
						var pad = this.padding;
						r.x += pad;
						r.y += pad;
						r.w -= pad;
						r.h -= pad;
						info.stbtt_GetGlyphHMetrics(glyph, ref advance, ref lsb);
						info.stbtt_GetGlyphBitmapBox(glyph, scale * this.h_oversample, scale * this.v_oversample, ref x0,
							ref y0, ref x1, ref y1);
						info.stbtt_MakeGlyphBitmapSubpixel(this.pixels + r.x + r.y * this.stride_in_bytes,
							(int)(r.w - this.h_oversample + 1), (int)(r.h - this.v_oversample + 1), this.stride_in_bytes,
							scale * this.h_oversample, scale * this.v_oversample, 0, 0, glyph);
						if (this.h_oversample > 1)
							Common.stbtt__h_prefilter(this.pixels + r.x + r.y * this.stride_in_bytes, r.w, r.h,
								this.stride_in_bytes, this.h_oversample);
						if (this.v_oversample > 1)
							Common.stbtt__v_prefilter(this.pixels + r.x + r.y * this.stride_in_bytes, r.w, r.h,
								this.stride_in_bytes, this.v_oversample);
						bc.x0 = (ushort)(short)r.x;
						bc.y0 = (ushort)(short)r.y;
						bc.x1 = (ushort)(short)(r.x + r.w);
						bc.y1 = (ushort)(short)(r.y + r.h);
						bc.xadvance = scale * advance;
						bc.xoff = x0 * recip_h + sub_x;
						bc.yoff = y0 * recip_v + sub_y;
						bc.xoff2 = (x0 + r.w) * recip_h + sub_x;
						bc.yoff2 = (y0 + r.h) * recip_v + sub_y;
						if (glyph == 0)
							missing_glyph = j;
					}
					else if (this.skip_missing != 0)
					{
						return_value = 0;
					}
					else if (r.was_packed != 0 && r.w == 0 && r.h == 0 && missing_glyph >= 0)
					{
						ranges[i].chardata_for_range[j] = ranges[i].chardata_for_range[missing_glyph];
					}
					else
					{
						return_value = 0;
					}

					++k;
				}
			}

			this.h_oversample = (uint)old_h_over;
			this.v_oversample = (uint)old_v_over;
			return return_value;
		}

		public void stbtt_PackFontRangesPackRects(stbrp_rect[] rects, int num_rects)
		{
			this.pack_info.stbrp_pack_rects(rects, num_rects);
		}

		public int stbtt_PackFontRanges(byte[] fontdata, int font_index,
			FakePtr<stbtt_pack_range> ranges, int num_ranges)
		{
			var info = new FontInfo();
			var i = 0;
			var j = 0;
			var n = 0;
			var return_value = 1;
			stbrp_rect[] rects;
			for (i = 0; i < num_ranges; ++i)
				for (j = 0; j < ranges[i].num_chars; ++j)
					ranges[i].chardata_for_range[j].x0 = ranges[i].chardata_for_range[j].y0 =
						ranges[i].chardata_for_range[j].x1 = ranges[i].chardata_for_range[j].y1 = 0;
			n = 0;
			for (i = 0; i < num_ranges; ++i)
				n += ranges[i].num_chars;
			rects = new stbrp_rect[n];
			for (i = 0; i < rects.Length; ++i)
				rects[i] = new stbrp_rect();
			if (rects == null)
				return 0;
			info.stbtt_InitFont(fontdata, Common.stbtt_GetFontOffsetForIndex(fontdata, font_index));
			n = stbtt_PackFontRangesGatherRects(info, ranges, num_ranges, rects);
			stbtt_PackFontRangesPackRects(rects, n);
			return_value = stbtt_PackFontRangesRenderIntoRects(info, ranges, num_ranges, rects);
			return return_value;
		}

		public int stbtt_PackFontRange(byte[] fontdata, int font_index, float font_size,
			int first_unicode_codepoint_in_range, int num_chars_in_range, stbtt_packedchar[] chardata_for_range)
		{
			var range = new stbtt_pack_range();
			range.first_unicode_codepoint_in_range = first_unicode_codepoint_in_range;
			range.array_of_unicode_codepoints = null;
			range.num_chars = num_chars_in_range;
			range.chardata_for_range = chardata_for_range;
			range.font_size = font_size;

			var ranges = new FakePtr<stbtt_pack_range>(range);
			return stbtt_PackFontRanges(fontdata, font_index, ranges, 1);
		}
	}
}