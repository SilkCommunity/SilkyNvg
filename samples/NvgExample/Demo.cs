using Silk.NET.Maths;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Images;
using SilkyNvg.Paths;
using SilkyNvg.Scissoring;
using SilkyNvg.Text;
using SilkyNvg.Transforms;
using System;

namespace NvgExample
{
    public class Demo : IDisposable
    {

        private const int ICON_SEARCH = 0x1F50D;
        private const int ICON_CIRCLED_CROSS = 0x2716;
        private const int ICON_CHEVRON_RIGHT = 0xE75E;
        private const int ICON_CHECK = 0x2713;
        private const int ICON_LOGIN = 0xE740;
        private const int ICON_TRASH = 0xE729;

        private readonly int _fontNormal, _fontBold, _fontIcons, _fontEmoji;
        private readonly int[] _images = new int[12];

        private readonly Nvg _nvg;

        private void DrawWindow(string title, float x, float y, float w, float h)
        {
            const float cornerRadius = 3.0f;

            _nvg.Save();

            _nvg.BeginPath();
            _nvg.RoundedRect(x, y, w, h, cornerRadius);
            _nvg.FillColour(_nvg.Rgba(28, 30, 34, 192));
            _nvg.Fill();

            Paint shadowPaint = Paint.BoxGradient(x, y + 2.0f, w, h, cornerRadius * 2.0f, 10.0f, _nvg.Rgba(0, 0, 0, 128), _nvg.Rgba(0, 0, 0, 0));
            _nvg.BeginPath();
            _nvg.Rect(x - 10.0f, y - 10.0f, w + 20.0f, h + 30.0f);
            _nvg.RoundedRect(x, y, w, h, cornerRadius);
            _nvg.PathWinding(Solidity.Hole);
            _nvg.FillPaint(shadowPaint);
            _nvg.Fill();

            Paint headerPaint = _nvg.LinearGradient(x, y, x, y + 15.0f, _nvg.Rgba(255, 255, 255, 8), _nvg.Rgba(0, 0, 0, 16));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, 30.0f, cornerRadius - 1.0f);
            _nvg.FillPaint(headerPaint);
            _nvg.Fill();
            _nvg.BeginPath();
            _nvg.MoveTo(x + 0.5f, y + 0.5f + 30.0f);
            _nvg.LineTo(x + 0.5f + w - 1.0f, y + 0.5f + 30.0f);
            _nvg.StrokeColour(_nvg.Rgba(0, 0, 0, 30));
            _nvg.Stroke();

            _nvg.FontSize(15.0f);
            _nvg.FontFace("sans-bold");
            _nvg.TextAlign(Align.Centre | Align.Middle);

            _nvg.FontBlur(2.0f);
            _nvg.FillColour(_nvg.Rgba(0, 0, 0, 128));
            _ = _nvg.Text(x + w / 2.0f, y + 16.0f + 1.0f, title);

            _nvg.FontBlur(0);
            _nvg.FillColour(_nvg.Rgba(220, 220, 220, 160));
            _ = _nvg.Text(x + w / 2.0f, y + 16.0f, title);

            _nvg.Restore();
        }

        private void DrawSearchBox(string text, float x, float y, float w, float h)
        {
            float cornerRadius = h / 2.0f - 1.0f;

            Paint bg = _nvg.BoxGradient(x, y + 1.5f, w, h, h / 2.0f, 5.0f, _nvg.Rgba(0, 0, 0, 16), _nvg.Rgba(0, 0, 0, 92));
            _nvg.BeginPath();
            _nvg.RoundedRect(x, y, w, h, cornerRadius);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, cornerRadius - 0.5f);
            _nvg.StrokeColour(_nvg.Rgba(0, 0, 0, 48));
            _nvg.Stroke();

            _nvg.BeginPath();
            _nvg.FontSize(h * 1.3f);
            _nvg.FontFace("icons");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 64));
            _nvg.TextAlign(Align.Centre | Align.Middle);
            _ = _nvg.Text(x + h * 0.55f, y + h * 0.55f, CpToUTF8(ICON_SEARCH));

            _nvg.FontSize(17.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 32));

            _nvg.TextAlign(Align.Left | Align.Middle);
            _ = _nvg.Text(x + h * 1.05f, y + h * 0.5f, text);

            _nvg.FontSize(h * 1.3f);
            _nvg.FontFace("icons");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 32));
            _nvg.TextAlign(Align.Centre | Align.Middle);
            _ = _nvg.Text(x + w - h * 0.55f, y + h * 0.55f, CpToUTF8(ICON_CIRCLED_CROSS));
        }

        private void DrawDropDown(string text, float x, float y, float w, float h)
        {
            const float cornerRadius = 4.0f;

            Paint bg = Paint.LinearGradient(x, y, x, y + h, _nvg.Rgba(255, 255, 255, 16), _nvg.Rgba(0, 0, 0, 16));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, h - 2.0f, cornerRadius - 1.0f);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, cornerRadius - 0.5f);
            _nvg.StrokeColour(_nvg.Rgba(0, 0, 0, 48));
            _nvg.Stroke();

            _nvg.FontSize(17.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 160));
            _nvg.TextAlign(Align.Left | Align.Middle);
            _nvg.Text(x + h * 0.3f, y + h * 0.5f, text);

            _nvg.FontSize(h * 1.3f);
            _nvg.FontFace("icons");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 64));
            _nvg.TextAlign(Align.Centre | Align.Middle);
            _nvg.Text(x + w - h * 0.5f, y + h * 0.5f, CpToUTF8(ICON_CHEVRON_RIGHT));
        }

        private void DrawLable(string text, float x, float y, float h)
        {
            _nvg.FontSize(15.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 128));

            _nvg.TextAlign(Align.Left | Align.Middle);
            _ = _nvg.Text(x, y + h * 0.5f, text);
        }

        private void DrawEditBoxBase(float x, float y, float w, float h)
        {
            Paint bg = _nvg.BoxGradient(x + 1.0f, y + 1.0f + 1.5f, w - 2.0f, h - 2.0f, 3.0f, 4.0f, _nvg.Rgba(255, 255, 255, 32), _nvg.Rgba(32, 32, 32, 32));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, h - 2.0f, 4.0f - 1.0f);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, 4.0f - 1.0f);
            _nvg.StrokeColour(_nvg.Rgba(0, 0, 0, 48));
            _nvg.Stroke();
        }

        private void DrawEditBox(string text, float x, float y, float w, float h)
        {
            DrawEditBoxBase(x, y, w, h);

            _nvg.FontSize(17.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 64));
            _nvg.TextAlign(Align.Left | Align.Middle);
            _nvg.Text(x + h * 0.3f, y + h * 0.5f, text);
        }

        private void DrawEditBoxNum(string text, string units, float x, float y, float w, float h)
        {
            DrawEditBoxBase(x, y, w, h);

            float uw = _nvg.TextBounds(0.0f, 0.0f, units, out _);

            _nvg.FontSize(15.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 64));
            _nvg.TextAlign(Align.Right | Align.Middle);
            _ = _nvg.Text(x + w - h * 0.3f, y + h * 0.5f, units);

            _nvg.FontSize(17.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 128));
            _nvg.TextAlign(Align.Right | Align.Middle);
            _ = _nvg.Text(x + w - uw - h * 0.5f, y + h * 0.5f, text);
        }

        private void DrawCheckBox(string text, float x, float y, float _, float h)
        {
            _nvg.FontSize(15.0f);
            _nvg.FontFace("sans");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 160));

            _nvg.TextAlign(Align.Left | Align.Middle);
            _nvg.Text(x + 28.0f, y + h * 0.5f, text);

            Paint bg = Paint.BoxGradient(x + 1.0f, y + (int)(h * 0.5f) - 9 + 1, 18.0f, 18.0f, 3.0f, 3.0f, _nvg.Rgba(0, 0, 0, 32), _nvg.Rgba(0, 0, 0, 92));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + 1.0f, y + (int)(h * 0.5f) - 9, 18.0f, 18.0f, 3.0f);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            _nvg.FontSize(33.0f);
            _nvg.FontFace("icons");
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 128));
            _nvg.TextAlign(Align.Centre | Align.Middle);
            _nvg.Text(x + 9.0f + 2.0f, y + h * 0.5f, CpToUTF8(ICON_CHECK));
        }

        private void DrawButton(int preIcon, string text, float x, float y, float w, float h, Colour col)
        {
            const float cornerRadius = 4.0f;
            float iw = 0.0f;

            Paint bg = _nvg.LinearGradient(x, y, x, y + h, _nvg.Rgba(255, 255, 255, IsBlack(col) ? (byte)16 : (byte)32), _nvg.Rgba(0, 0, 0, IsBlack(col) ? (byte)16 : (byte)32));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + 1.0f, y + 1.0f, w - 2.0f, h - 2.0f, cornerRadius - 1.0f);
            if (!IsBlack(col))
            {
                _nvg.FillColour(col);
                _nvg.Fill();
            }
            _nvg.FillPaint(bg);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.RoundedRect(x + 0.5f, y + 0.5f, w - 1.0f, h - 1.0f, cornerRadius - 0.5f);
            _nvg.StrokeColour(_nvg.Rgba(0, 0, 0, 48));
            _nvg.Stroke();

            _nvg.FontSize(17.0f);
            _nvg.FontFace("sans-bold");
            float tw = _nvg.TextBounds(0.0f, 0.0f, text, out _);
            if (preIcon != 0)
            {
                _nvg.FontSize(h * 1.3f);
                _nvg.FontFace("icons");
                iw = _nvg.TextBounds(0.0f, 0.0f, CpToUTF8(preIcon), out _);
                iw += h * 0.15f;
            }

            if (preIcon != 0)
            {
                _nvg.FontSize(h * 1.3f);
                _nvg.FontFace("icons");
                _nvg.FillColour(_nvg.Rgba(255, 255, 255, 96));
                _nvg.TextAlign(Align.Left | Align.Middle);
                _nvg.Text(x + w * 0.5f - tw * 0.5f - iw * 0.75f, y + h * 0.5f, CpToUTF8(preIcon));
            }

            _nvg.FontSize(17.0f);
            _nvg.FontFace("sans-bold");
            _nvg.TextAlign(Align.Left | Align.Middle);
            _nvg.FillColour(_nvg.Rgba(0, 0, 0, 160));
            _nvg.Text(x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f - 1.0f, text);
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 160));
            _nvg.Text(x + w * 0.5f - tw * 0.5f + iw * 0.25f, y + h * 0.5f, text);
        }

        private void DrawSlider(float pos, float x, float y, float w, float h)
        {
            float cy = y + (int)(h * 0.5f);
            float kr = (int)(h * 0.25f);

            _nvg.Save();

            Paint bg = _nvg.BoxGradient(x, cy - 2.0f + 1.0f, w, 4.0f, 2.0f, 2.0f, _nvg.Rgba(0, 0, 0, 32), _nvg.Rgba(0, 0, 0, 128));
            _nvg.BeginPath();
            _nvg.RoundedRect(x, cy - 2.0f, w, 4.0f, 2.0f);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            bg = _nvg.RadialGradient(x + (int)(pos * w), cy + 1.0f, kr - 3.0f, kr + 3.0f, _nvg.Rgba(0, 0, 0, 64), _nvg.Rgba(0, 0, 0, 0));
            _nvg.BeginPath();
            _nvg.Rect(x + (int)(pos * w) - kr - 5, cy - kr - 5, kr * 2.0f + 5.0f + 5.0f, kr * 2.0f + 5.0f + 5.0f + 3.0f);
            _nvg.Circle(x + (int)(pos * w), cy, kr);
            _nvg.PathWinding(Solidity.Hole);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            Paint knob = Paint.LinearGradient(x, cy - kr, x, cy + kr, _nvg.Rgba(255, 255, 255, 16), _nvg.Rgba(0, 0, 0, 16));
            _nvg.BeginPath();
            _nvg.Circle(x + (int)(pos * w), cy, kr - 1.0f);
            _nvg.FillColour(_nvg.Rgba(40, 43, 48, 255));
            _nvg.Fill();
            _nvg.FillPaint(knob);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.Circle(x + (int)(pos * w), cy, kr - 0.5f);
            _nvg.StrokeColour(_nvg.Rgba(0, 0, 0, 92));
            _nvg.Stroke();

            _nvg.Restore();
        }

        private void DrawEyes(float x, float y, float w, float h, float mx, float my, float t)
        {
            float ex = w * 0.23f;
            float ey = h * 0.5f;
            float lx = x + ex;
            float ly = y + ey;
            float rx = x + w - ex;
            float ry = y + ey;
            float br = MathF.Min(ex, ey) * 0.5f;
            float blink = 1.0f - MathF.Pow(MathF.Sin(t * 0.5f), 200.0f) * 0.8f;

            Paint bg = _nvg.LinearGradient(x, y + (h * 0.5f), x + (w * 0.1f), y + h, _nvg.Rgba(0, 0, 0, 32), _nvg.Rgba(0, 0, 0, 16));
            _nvg.BeginPath();
            _nvg.Ellipse(lx + 3.0f, ly + 16.0f, ex, ey);
            _nvg.Ellipse(rx + 3.0f, ry + 16.0f, ex, ey);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            bg = _nvg.LinearGradient(x, y + (h * 0.25f), x + (w * 0.1f), y + h, _nvg.Rgba(220, 220, 220, 255), _nvg.Rgba(128, 128, 128, 255));
            _nvg.BeginPath();
            _nvg.Ellipse(lx, ly, ex, ey);
            _nvg.Ellipse(rx, ry, ex, ey);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            float dx = (mx - rx) / (ex * 10.0f);
            float dy = (my - ry) / (ey * 10.0f);
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }
            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            _nvg.BeginPath();
            _nvg.Ellipse(lx + dx, ly + dy + ey * 0.25f * (1.0f - blink), br, br * blink);
            _nvg.FillColour(_nvg.Rgba(32, 32, 32, 255));
            _nvg.Fill();

            dx = (mx - rx) / (ex * 10.0f);
            dy = (my - ry) / (ey * 10.0f);
            d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }
            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            _nvg.BeginPath();
            _nvg.Ellipse(rx + dx, ry + dy + ey * 0.25f * (1.0f - blink), br, br * blink);
            _nvg.FillColour(_nvg.Rgba(32, 32, 32, 255));
            _nvg.Fill();

            Paint gloss = _nvg.RadialGradient(lx - ex * 0.25f, ly - ey * 0.5f, ex * 0.1f, ex * 0.75f, _nvg.Rgba(255, 255, 255, 128), _nvg.Rgba(255, 255, 255, 0));
            _nvg.BeginPath();
            _nvg.Ellipse(lx, ly, ex, ey);
            _nvg.FillPaint(gloss);
            _nvg.Fill();

            gloss = _nvg.RadialGradient(rx - ex * 0.25f, ry - ey * 0.5f, ex * 0.1f, ex * 0.75f, _nvg.Rgba(255, 255, 255, 128), _nvg.Rgba(255, 255, 255, 0));
            _nvg.BeginPath();
            _nvg.Ellipse(rx, ry, ex, ey);
            _nvg.FillPaint(gloss);
            _nvg.Fill();
        }

        private void DrawGraph(float x, float y, float w, float h, float t)
        {
            float dx = w / 5.0f;

            Span<float> samples = stackalloc float[]
            {
                (1 + MathF.Sin(t * 1.2345f + MathF.Cos(t * 0.33457f) * 0.44f)) * 0.5f,
                (1 + MathF.Sin(t * 0.68363f + MathF.Cos(t * 1.3f) * 1.55f)) * 0.5f,
                (1 + MathF.Sin(t * 1.1642f + MathF.Cos(t * 0.33457f) * 1.24f)) * 0.5f,
                (1 + MathF.Sin(t * 0.56345f + MathF.Cos(t * 1.63f) * 0.14f)) * 0.5f,
                (1 + MathF.Sin(t * 1.6245f + MathF.Cos(t * 0.254f) * 0.3f)) * 0.5f,
                (1 + MathF.Sin(t * 0.345f + MathF.Cos(t * 0.03f) * 0.6f)) * 0.5f
            };

            Span<float> sx = stackalloc float[6];
            Span<float> sy = stackalloc float[6];

            for (int i = 0; i < 6; i++)
            {
                sx[i] = x + i * dx;
                sy[i] = y + h * samples[i] * 0.8f;
            }

            Paint bg = Paint.LinearGradient(x, y, x, y + h, new Colour(0, 160, 192, 0), new Colour(0, 160, 192, 64));
            _nvg.BeginPath();
            _nvg.MoveTo(sx[0], sy[0]);
            for (int i = 1; i < 6; i++)
            {
                _nvg.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }
            _nvg.LineTo(x + w, y + h);
            _nvg.LineTo(x, y + h);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.MoveTo(sx[0], sy[0] + 2);
            for (int i = 1; i < 6; i++)
            {
                _nvg.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1] + 2, sx[i] - dx * 0.5f, sy[i] + 2, sx[i], sy[i] + 2);
            }
            _nvg.StrokeColour(new Colour(0, 0, 0, 32));
            _nvg.StrokeWidth(3.0f);
            _nvg.Stroke();

            _nvg.BeginPath();
            _nvg.MoveTo(sx[0], sy[0]);
            for (int i = 1; i < 6; i++)
            {
                _nvg.BezierTo(sx[i - 1] + dx * 0.5f, sy[i - 1], sx[i] - dx * 0.5f, sy[i], sx[i], sy[i]);
            }
            _nvg.StrokeColour(new Colour(0, 160, 192, 255));
            _nvg.StrokeWidth(3.0f);
            _nvg.Stroke();

            for (int i = 0; i < 6; i++)
            {
                bg = Paint.RadialGradient(sx[i], sy[i] + 2, 3.0f, 8.0f, new Colour(0, 0, 0, 32), new Colour(0, 0, 0, 0));
                _nvg.BeginPath();
                _nvg.Rect(sx[i] - 10, sy[i] - 10 + 2, 20, 20);
                _nvg.FillPaint(bg);
                _nvg.Fill();
            }

            _nvg.BeginPath();
            for (int i = 0; i < 6; i++)
            {
                _nvg.Circle(sx[i], sy[i], 4.0f);
            }
            _nvg.FillColour(new Colour(0, 160, 192, 255));
            _nvg.Fill();
            _nvg.BeginPath();
            for (int i = 0; i < 6; i++)
            {
                _nvg.Circle(sx[i], sy[i], 2.0f);
            }
            _nvg.FillColour(new Colour(220, 220, 220, 255));
            _nvg.Fill();
        }

        private void DrawSpinner(float cx, float cy, float r, float t)
        {
            float a0 = 0.0f + t * 6.0f;
            float a1 = MathF.PI + t * 6.0f;
            float r0 = r;
            float r1 = r * 0.75f;

            _nvg.Save();

            _nvg.BeginPath();
            _nvg.Arc(cx, cy, r0, a0, a1, Winding.Cw);
            _nvg.Arc(cx, cy, r1, a1, a0, Winding.Ccw);
            _nvg.ClosePath();
            float ax = cx + MathF.Cos(a0) * (r0 + r1) * 0.5f;
            float ay = cy + MathF.Sin(a0) * (r0 + r1) * 0.5f;
            float bx = cx + MathF.Cos(a0) * (r0 + r1) * 0.5f;
            float by = cy + MathF.Sin(a0) * (r0 + r1) * 0.5f;
            Paint paint = _nvg.LinearGradient(ax, ay, bx, by, _nvg.Rgba(0, 0, 0, 0), _nvg.Rgba(0, 0, 0, 128));
            _nvg.FillPaint(paint);
            _nvg.Fill();

            _nvg.Restore();
        }

        private void DrawThumbnails(float x, float y, float w, float h, int[] images, float t)
        {
            const float cornerRadius = 3.0f;
            const float thumb = 60.0f;
            const float arry = 30.5f;
            float stackh = (images.Length / 2) * (thumb + 10.0f) + 10.0f;
            float u = (1.0f + MathF.Cos(t * 0.5f)) * 0.5f;
            float u2 = (1.0f - MathF.Cos(t * 0.2f)) * 0.5f;

            _nvg.Save();

            Paint shadowPaint = _nvg.BoxGradient(x, y + 4.0f, w, h, cornerRadius * 2.0f, 20.0f, _nvg.Rgba(0, 0, 0, 128), _nvg.Rgba(0, 0, 0, 0));
            _nvg.BeginPath();
            _nvg.Rect(x - 10.0f, y - 10.0f, w + 20.0f, h + 30.0f);
            _nvg.RoundedRect(x, y, w, h, cornerRadius);
            _nvg.PathWinding(Solidity.Hole);
            _nvg.FillPaint(shadowPaint);
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.RoundedRect(x, y, w, h, cornerRadius);
            _nvg.MoveTo(x - 10.0f, y + arry);
            _nvg.LineTo(x + 1.0f, y + arry - 11.0f);
            _nvg.LineTo(x + 1.0f, y + arry + 11.0f);
            _nvg.FillColour(_nvg.Rgba(200, 200, 200, 255));
            _nvg.Fill();

            _nvg.Save();
            _nvg.Scissor(x, y, w, h);
            _nvg.Translate(0, -(stackh - h) * u);

            float dv = 1.0f / (float)(images.Length - 1);

            for (uint i = 0; i < images.Length; i++)
            {
                float iw, ih;
                float ix, iy;
                float tx = x + 10.0f;
                float ty = y + 10.0f;
                tx += (i % 2) * (thumb + 10.0f);
                ty += (i / 2) * (thumb + 10.0f);
                _nvg.ImageSize(images[i], out uint imgW, out uint imgH);
                if (imgW < imgH)
                {
                    iw = thumb;
                    ih = iw * (float)imgH / (float)imgW;
                    ix = 0.0f;
                    iy = -(ih - thumb) * 0.5f;
                }
                else
                {
                    ih = thumb;
                    iw = ih * (float)imgW / (float)imgH;
                    ix = -(iw - thumb) * 0.5f;
                    iy = 0.0f;
                }

                float v = i * dv;
                float a = Clamp((u2 - v) / dv, 0.0f, 1.0f);

                if (a < 1.0f)
                {
                    DrawSpinner(tx + thumb / 2.0f, ty + thumb / 2.0f, thumb * 0.25f, t);
                }

                Paint imgPaint = Paint.ImagePattern(tx + ix, ty + iy, iw, ih, 0.0f / 180.0f * MathF.PI, images[i], a);
                _nvg.BeginPath();
                _nvg.RoundedRect(tx, ty, thumb, thumb, 5.0f);
                _nvg.FillPaint(imgPaint);
                _nvg.Fill();

                shadowPaint = Paint.BoxGradient(tx - 1.0f, ty, thumb + 2.0f, thumb + 2.0f, 5.0f, 3.0f, _nvg.Rgba(0, 0, 0, 128), _nvg.Rgba(0, 0, 0, 0));
                _nvg.BeginPath();
                _nvg.Rect(tx - 5.0f, ty - 5.0f, thumb + 10.0f, thumb + 10.0f);
                _nvg.RoundedRect(tx, ty, thumb, thumb, 6.0f);
                _nvg.PathWinding(Solidity.Hole);
                _nvg.FillPaint(shadowPaint);
                _nvg.Fill();

                _nvg.BeginPath();
                _nvg.RoundedRect(tx + 0.5f, ty + 0.5f, thumb - 1.0f, thumb - 1.0f, 4.0f - 0.5f);
                _nvg.StrokeWidth(1.0f);
                _nvg.StrokeColour(_nvg.Rgba(255, 255, 255, 192));
                _nvg.Fill();
            }
            _nvg.Restore();

            Paint fadePaint = _nvg.LinearGradient(x, y, x, y + 6.0f, _nvg.Rgba(200, 200, 200, 255), _nvg.Rgba(200, 200, 200, 0));
            _nvg.BeginPath();
            _nvg.Rect(x + 4.0f, y, w - 8.0f, 6.0f);
            _nvg.FillPaint(fadePaint);
            _nvg.Fill();

            fadePaint = _nvg.LinearGradient(x, y + h, x, y + h - 6.0f, _nvg.Rgba(200, 200, 200, 255), _nvg.Rgba(200, 200, 200, 0));
            _nvg.BeginPath();
            _nvg.Rect(x + 4.0f, y + h - 6.0f, w - 8.0f, 6.0f);
            _nvg.FillPaint(fadePaint);
            _nvg.Fill();

            shadowPaint = _nvg.BoxGradient(x + w - 12.0f + 1.0f, y + 4.0f + 1.0f, 8.0f, h - 8.0f, 3.0f, 4.0f, _nvg.Rgba(0, 0, 0, 32), _nvg.Rgba(0, 0, 0, 92));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + w - 12.0f, y + 4.0f, 8.0f, h - 8.0f, 3.0f);
            _nvg.FillPaint(shadowPaint);
            _nvg.Fill();

            float scrollH = (h / stackh) * (h - 8.0f);
            shadowPaint = _nvg.BoxGradient(x + w - 12.0f - 1.0f, y + 4.0f + (h - 8.0f - scrollH) * u - 1.0f, 8.0f, scrollH, 3.0f, 4.0f, _nvg.Rgba(220, 220, 220, 255), _nvg.Rgba(128, 128, 128, 255));
            _nvg.BeginPath();
            _nvg.RoundedRect(x + w - 12.0f + 1.0f, y + 4.0f + 1.0f + (h - 8.0f - scrollH) * u, 8.0f - 2.0f, scrollH - 2.0f, 2.0f);
            _nvg.FillPaint(shadowPaint);
            _nvg.Fill();

            _nvg.Restore();
        }

        private void DrawColourwheel(float x, float y, float w, float h, float t)
        {
            float hue = MathF.Sin(t * 0.12f);

            _nvg.Save();

            float cx = x + w * 0.5f;
            float cy = y + h * 0.5f;
            float r1 = (w < h ? w : h) * 0.5f - 5.0f;
            float r0 = r1 - 20.0f;
            float aeps = 0.5f / r1;

            Paint paint;
            float ax, ay, bx, by;

            for (int i = 0; i < 6; i++)
            {
                float a0 = (float)i / 6.0f * MathF.PI * 2.0f - aeps;
                float a1 = (float)(i + 1.0f) / 6.0f * MathF.PI * 2.0f + aeps;
                _nvg.BeginPath();
                _nvg.Arc(cx, cy, r0, a0, a1, Winding.Cw);
                _nvg.Arc(cx, cy, r1, a1, a0, Winding.Ccw);
                _nvg.ClosePath();
                ax = cx + MathF.Cos(a0) * (r0 + 1) * 0.5f;
                ay = cy + MathF.Sin(a0) * (r0 + 1) * 0.5f;
                bx = cx + MathF.Cos(a1) * (r0 + 1) * 0.5f;
                by = cy + MathF.Sin(a1) * (r0 + 1) * 0.5f;
                paint = _nvg.LinearGradient(ax, ay, bx, by, _nvg.Hsla(a0 / (MathF.PI * 2), 1.0f, 0.55f, 255), _nvg.Hsla(a1 / (MathF.PI * 2), 1.0f, 0.55f, 255));
                _nvg.FillPaint(paint);
                _nvg.Fill();
            }

            _nvg.BeginPath();
            _nvg.Circle(cx, cy, r0 - 0.5f);
            _nvg.Circle(cx, cy, r1 + 0.5f);
            _nvg.StrokeColour(new Colour(0, 0, 0, 64));
            _nvg.StrokeWidth(1.0f);
            _nvg.Stroke();

            _nvg.Save();
            _nvg.Translate(cx, cy);
            _nvg.Rotate(hue * MathF.PI * 2);

            _nvg.StrokeWidth(2.0f);
            _nvg.BeginPath();
            _nvg.Rect(r0 - 1, -3, r1 - r0 + 2, 6);
            _nvg.StrokeColour(new Colour(255, 255, 255, 255));
            _nvg.Stroke();

            paint = _nvg.BoxGradient(r0 - 3, -5, r1 - r0 + 6, 10, 2, 4, new Colour(0, 0, 0, 128), new Colour(0, 0, 0, 0));
            _nvg.BeginPath();
            _nvg.Rect(r0 - 2 - 10, -4 - 10, r1 - r0 + 4 + 20, 8 + 20);
            _nvg.Rect(r0 - 2, -4, r1 - r0 + 4, 8);
            _nvg.PathWinding(Solidity.Hole);
            _nvg.FillPaint(paint);
            _nvg.Fill();

            float r = r0 - 6;
            ax = MathF.Cos(120.0f / 180.0f * MathF.PI) * r;
            ay = MathF.Sin(120.0f / 180.0f * MathF.PI) * r;
            bx = MathF.Cos(-120.0f / 180.0f * MathF.PI) * r;
            by = MathF.Sin(-120.0f / 180.0f * MathF.PI) * r;
            _nvg.BeginPath();
            _nvg.MoveTo(r, 0);
            _nvg.LineTo(ax, ay);
            _nvg.LineTo(bx, by);
            _nvg.ClosePath();
            paint = _nvg.LinearGradient(r, 0, ax, ay, _nvg.Hsla(hue, 1.0f, 0.5f, 255), _nvg.Rgba(255, 255, 255, 255));
            _nvg.FillPaint(paint);
            _nvg.Fill();
            paint = _nvg.LinearGradient((r + ax) * 0.5f, (0 + ay) * 0.5f, bx, by, new Colour(0, 0, 0, 0), new Colour(0, 0, 0, 255));
            _nvg.FillPaint(paint);
            _nvg.Fill();
            _nvg.StrokeColour(new Colour(0, 0, 0, 64));
            _nvg.Stroke();

            ax = MathF.Cos(120.0f / 180.0f * MathF.PI) * r * 0.3f;
            ay = MathF.Sin(120.0f / 180.0f * MathF.PI) * r * 0.4f;
            _nvg.StrokeWidth(2.0f);
            _nvg.BeginPath();
            _nvg.Circle(ax, ay, 5);
            _nvg.StrokeColour(new Colour(255, 255, 255, 192));
            _nvg.Stroke();

            paint = _nvg.RadialGradient(ax, ay, 7, 9, new Colour(0, 0, 0, 64), new Colour(0, 0, 0, 0));
            _nvg.BeginPath();
            _nvg.Rect(ax - 20, ay - 20, 40, 40);
            _nvg.Circle(ax, ay, 7);
            _nvg.PathWinding(Solidity.Hole);
            _nvg.FillPaint(paint);
            _nvg.Fill();

            _nvg.Restore();
            _nvg.Restore();
        }

        private unsafe void DrawLines(float x, float y, float w, float h, float t)
        {
            float pad = 5.0f;
            float s = w / 9.0f - pad * 2;
            float* pts = stackalloc float[4 * 2];
            LineCap[] joins = { LineCap.Miter, LineCap.Round, LineCap.Bevel };
            LineCap[] caps = { LineCap.Butt, LineCap.Round, LineCap.Square };

            _nvg.Save();
            pts[0] = -s * 0.25f + MathF.Cos(t * 0.3f) * s * 0.5f;
            pts[1] = MathF.Sin(t * 0.3f) * s * 0.5f;
            pts[2] = -s * 0.25f;
            pts[3] = 0;
            pts[4] = s * 0.25f;
            pts[5] = 0;
            pts[6] = s * 0.25f + MathF.Cos(-t * 0.3f) * s * 0.5f;
            pts[7] = MathF.Sin(-t * 0.3f) * s * 0.5f;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float fx = x + s * 0.5f + (i * 3 + j) / 9.0f * w + pad;
                    float fy = y - s * 0.5f + pad;

                    _nvg.LineCap(caps[i]);
                    _nvg.LineJoin(joins[j]);

                    _nvg.StrokeWidth(s * 0.3f);
                    _nvg.StrokeColour(new Colour(0, 0, 0, 160));
                    _nvg.BeginPath();
                    _nvg.MoveTo(fx + pts[0], fy + pts[1]);
                    _nvg.LineTo(fx + pts[2], fy + pts[3]);
                    _nvg.LineTo(fx + pts[4], fy + pts[5]);
                    _nvg.LineTo(fx + pts[6], fy + pts[7]);
                    _nvg.Stroke();

                    _nvg.LineCap(LineCap.Butt);
                    _nvg.LineJoin(LineCap.Bevel);

                    _nvg.StrokeWidth(1.0f);
                    _nvg.StrokeColour(new Colour(0, 192, 255, 255));
                    _nvg.BeginPath();
                    _nvg.MoveTo(fx + pts[0], fy + pts[1]);
                    _nvg.LineTo(fx + pts[2], fy + pts[3]);
                    _nvg.LineTo(fx + pts[4], fy + pts[5]);
                    _nvg.LineTo(fx + pts[6], fy + pts[7]);
                    _nvg.Stroke();
                }
            }

            _nvg.Restore();
        }

        public Demo(Nvg nvg)
        {
            _nvg = nvg;

            for (uint i = 0; i < 12; i++)
            {
                string file = "./images/image" + i + ".jpg";
                _images[i] = _nvg.CreateImage(file, 0);
                if (_images[i] == 0)
                {
                    Console.Error.WriteLine("Could not load " + file);
                    Environment.Exit(-1);
                }
            }

            _fontIcons = _nvg.CreateFont("icons", "./fonts/entypo.ttf");
            if (_fontIcons == -1)
            {
                Console.Error.WriteLine("Could not add font icons.");
                Environment.Exit(-1);
            }
            _fontNormal = _nvg.CreateFont("sans", "./fonts/Roboto-Regular.ttf");
            if (_fontIcons == -1)
            {
                Console.Error.WriteLine("Could not add font regular.");
                Environment.Exit(-1);
            }
            _fontBold = _nvg.CreateFont("sans-bold", "./fonts/Roboto-Bold.ttf");
            if (_fontIcons == -1)
            {
                Console.Error.WriteLine("Could not add font bold.");
                Environment.Exit(-1);
            }
            _fontEmoji = _nvg.CreateFont("emoji", "./fonts/NotoEmoji-Regular.ttf");
            if (_fontIcons == -1)
            {
                Console.Error.WriteLine("Could not add font emoji.");
                Environment.Exit(-1);
            }

            _ = _nvg.AddFallbackFontId(_fontNormal, _fontEmoji);
            _ = _nvg.AddFallbackFontId(_fontBold, _fontEmoji);
        }

        public void Dispose()
        {
            for (uint i = 0; i < 12; i++)
            {
                _nvg.DeleteImage(_images[i]);
            }
        }

        private void DrawParagraph(float x, float y, float width, float mx, float my)
        {
            string text = "This is a longer chunk of text.\n  \n"
                + "  Would have used lorem ipsum but she    was busy jumping over the lazy dog with the fox and all the men who came to the aid of the party.";
            string hoverText = "Hover your mouse over the text to see the calculated caret position.";
            uint gutter = 0;
            uint lnum = 0;
            float gx = 0.0f, gy = 0.0f;
            Rectangle<float> bounds;
            float px;

            _nvg.Save();

            _nvg.FontSize(15.0f);
            _nvg.FontFace("sans");
            _nvg.TextAlign(Align.Left | Align.Top);
            _nvg.TextMetrics(out _, out _, out float lineh);

            string start = text;
            string end = null;
            int nrows;
            while ((nrows = _nvg.TextBreakLines(start, end, width, out TextRow[] rows, 3)) != 0)
            {
                for (uint i = 0; i < nrows; i++)
                {
                    TextRow row = rows[i];
                    bool hit = mx > x && mx < (x + width) && my >= y && my < (y + lineh);

                    _nvg.BeginPath();
                    _nvg.FillColour(_nvg.Rgba(255, 255, 255, hit ? (byte)64 : (byte)16));
                    _nvg.Rect(x + row.MinX, y, row.MaxX - row.MinX, lineh);
                    _nvg.Fill();

                    _nvg.FillColour(Colour.White);
                    _ = _nvg.Text(x, y, row.Start, row.End.Length != 0 ? row.End : null);

                    if (hit)
                    {
                        float caretX = (mx < x + row.Width / 2.0f) ? x : x + row.Width;
                        px = x;
                        int glyphCount = _nvg.TextGlyphPositions(x, y, row.Start, row.End, out GlyphPosition[] glyps, 100);
                        for (uint j = 0; j < glyphCount; j++)
                        {
                            float x0 = glyps[j].X;
                            float x1 = (j + 1 < glyphCount) ? glyps[j + 1].X : x + row.Width;
                            gx = x0 * 0.3f + x1 * 0.7f;
                            if (mx >= px && mx < gx)
                            {
                                caretX = glyps[j].X;
                            }
                            px = gx;
                        }
                        _nvg.BeginPath();
                        _nvg.FillColour(_nvg.Rgba(255, 192, 0, 255));
                        _nvg.Rect(caretX, y, 1.0f, lineh);
                        _nvg.Fill();

                        gutter = lnum + 1;
                        gx = x - 10.0f;
                        gy = y + lineh / 2.0f;
                    }

                    lnum++;
                    y += lineh;
                }

                start = rows[^1].Next;
            }

            if (gutter != 0)
            {
                _nvg.FontSize(12.0f);
                _nvg.TextAlign(Align.Right | Align.Middle);

                _nvg.TextBounds(gx, gy, gutter.ToString(), out bounds);

                _nvg.BeginPath();
                _nvg.FillColour(_nvg.Rgba(255, 192, 0, 255));
                _nvg.RoundedRect(bounds.Origin.X - 4.0f, bounds.Origin.Y - 2.0f, bounds.Size.X + 8.0f, bounds.Size.Y + 4.0f, (bounds.Size.Y + 4.0f) / 2.0f - 1.0f);
                _nvg.Fill();

                _nvg.FillColour(_nvg.Rgba(32, 32, 32, 255));
                _ = _nvg.Text(gx, gy, gutter.ToString());
            }

            y += 20.0f;

            _nvg.FontSize(11.0f);
            _nvg.TextAlign(Align.Left | Align.Top);
            _nvg.TextLineHeight(1.2f);

            _nvg.TextBoxBounds(x, y, 150.0f, hoverText, out bounds);

            gx = Clamp(mx, bounds.Origin.X, bounds.Max.X) - mx;
            gy = Clamp(my, bounds.Origin.Y, bounds.Max.Y) - my;
            float a = MathF.Sqrt((gx * gx) + (gy * gy)) / 30.0f;
            a = Clamp(a, 0.0f, 1.0f);
            _nvg.GlobalAlpha(a);

            _nvg.BeginPath();
            _nvg.FillColour(_nvg.Rgba(220, 220, 220, 255));
            _nvg.RoundedRect(new Rectangle<float>(bounds.Origin - new Vector2D<float>(2.0f), bounds.Size + new Vector2D<float>(4.0f)), 3.0f);
            px = (bounds.Max.X + bounds.Origin.X) / 2.0f;
            _nvg.MoveTo(px, bounds.Origin.Y - 10.0f);
            _nvg.LineTo(px + 7.0f, bounds.Origin.Y + 1.0f);
            _nvg.LineTo(px - 7.0f, bounds.Origin.Y + 1.0f);
            _nvg.Fill();

            _nvg.FillColour(_nvg.Rgba(0, 0, 0, 220));
            _nvg.TextBox(x, y, 150.0f, hoverText);

            _nvg.Restore();
        }

        private void DrawWidths(float x, float y, float width)
        {
            _nvg.Save();

            _nvg.StrokeColour(Colour.Black);

            for (uint i = 0; i < 20; i++)
            {
                float w = (i + 0.5f) * 0.1f;
                _nvg.StrokeWidth(w);
                _nvg.BeginPath();
                _nvg.MoveTo(x, y);
                _nvg.LineTo(x + width, y + width * 0.3f);
                _nvg.Stroke();
                y += 10.0f;
            }

            _nvg.Restore();
        }

        private void DrawCaps(float x, float y, float width)
        {
            const float lineWidth = 8.0f;

            LineCap[] caps =
            {
                LineCap.Butt,
                LineCap.Round,
                LineCap.Square
            };

            _nvg.Save();

            _nvg.BeginPath();
            _nvg.Rect(x - lineWidth / 2.0f, y, width + lineWidth, 40.0f);
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 32));
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.Rect(x, y, width, 40.0f);
            _nvg.FillColour(_nvg.Rgba(255, 255, 255, 32));
            _nvg.Fill();

            _nvg.StrokeWidth(lineWidth);
            for (uint i = 0; i < 3; i++)
            {
                _nvg.LineCap(caps[i]);
                _nvg.StrokeColour(Colour.Black);
                _nvg.BeginPath();
                _nvg.MoveTo(x, y + i * 10.0f + 5.0f);
                _nvg.LineTo(x + width, y + i * 10.0f + 5.0f);
                _nvg.Stroke();
            }

            _nvg.Restore();
        }

        private void DrawScissor(float x, float y, float t)
        {
            _nvg.Save();

            _nvg.Translate(x, y);
            _nvg.Rotate(_nvg.DegToRad(5.0f));
            _nvg.BeginPath();
            _nvg.Rect(-20.0f, -20.0f, 60.0f, 40.0f);
            _nvg.FillColour(Colour.Red);
            _nvg.Fill();
            _nvg.Scissor(-20.0f, -20.0f, 60.0f, 40.0f);

            _nvg.Translate(40.0f, 0.0f);
            _nvg.Rotate(t);

            _nvg.Save();
            _nvg.ResetScissor();
            _nvg.BeginPath();
            _nvg.Rect(-20.0f, -10.0f, 60.0f, 30.0f);
            _nvg.FillColour(_nvg.Rgba(255, 128, 0, 64));
            _nvg.Fill();
            _nvg.Restore();

            _nvg.IntersectScissor(-20.0f, -10.0f, 60.0f, 30.0f);
            _nvg.BeginPath();
            _nvg.Rect(-20.0f, -10.0f, 60.0f, 30.0f);
            _nvg.FillColour(_nvg.Rgba(255, 128, 0, 255));
            _nvg.Fill();

            _nvg.Restore();
        }

        public void Render(float mx, float my, float width, float height, float t, bool blowup)
        {
            DrawEyes(width - 250.0f, 50.0f, 150.0f, 100.0f, mx, my, t);
            DrawParagraph(width - 450.0f, 50.0f, 150.0f, mx, my);
            DrawGraph(0.0f, height / 2.0f, width, height / 2.0f, t);
            DrawColourwheel(width - 300.0f, height - 300.0f, 250.0f, 250.0f, t);
            DrawLines(120.0f, height - 50.0f, 600.0f, 50.0f, t);

            DrawWidths(10, 50, 30);

            DrawCaps(10, 300, 30);

            DrawScissor(50.0f, height - 80.0f, t);

            _nvg.Save();
            if (blowup)
            {
                _nvg.Rotate(MathF.Sin(t * 0.3f) * 5.0f / 180.0f * MathF.PI);
                _nvg.Scale(2.0f, 2.0f);
            }

            DrawWindow("Widgets 'n' Stuff", 50.0f, 50.0f, 300.0f, 400.0f);
            float x = 60.0f;
            float y = 95.0f;
            DrawSearchBox("Search...", x, y, 280.0f, 25.0f);
            y += 40.0f;
            DrawDropDown("Effects:", x, y, 280.0f, 28.0f);
            float popY = y + 14.0f;
            y += 45.0f;

            DrawLable("Login", x, y, 20.0f);
            y += 25.0f;
            DrawEditBox("Email", x, y, 280.0f, 28.0f);
            y += 35.0f;
            DrawEditBox("Password", x, y, 280.0f, 28.0f);
            y += 38.0f;
            DrawCheckBox("Remember me", x, y, 140.0f, 28.0f);
            DrawButton(ICON_LOGIN, "Sign in", x + 138.0f, y, 140.0f, 28.0f, _nvg.Rgba(0, 96, 128, 255));
            y += 45.0f;

            DrawLable("Diameter", x, y, 20.0f);
            y += 25.0f;
            DrawEditBoxNum("123.00", "px", x + 180.0f, y, 100.0f, 28.0f);
            DrawSlider(0.4f, x, y, 170.0f, 28.0f);
            y += 55.0f;

            DrawButton(ICON_TRASH, "Delete", x, y, 160.0f, 28.0f, _nvg.Rgba(128, 16, 8, 255));
            DrawButton(0, "Cancel", x + 170.0f, y, 110.0f, 28.0f, _nvg.Rgba(0, 0, 0, 0));

            DrawThumbnails(365.0f, popY - 30.0f, 160.0f, 300.0f, _images, t);

            _nvg.Restore();
        }

        private static float Clamp(float a, float min, float max)
        {
            return (a > min) ? ((a < max) ? a : max) : min;
        }

        private static string CpToUTF8(int cp)
        {
            int n = 0;
            if (cp < 0x80)
            {
                n = 1;
            }
            else if (cp < 0x800)
            {
                n = 2;
            }
            else if (cp < 0x10000)
            {
                n = 3;
            }
            else if (cp < 0x200000)
            {
                n = 4;
            }
            else if (cp < 0x4000000)
            {
                n = 5;
            }
            else if (cp <= 0x7fffffff)
            {
                n = 6;
            }

            char[] str = new char[8];
            str[n] = '\0';

            switch (n)
            {
                case 6:
                    str[5] = (char)(0x80 | (cp & 0x3f));
                    cp >>= 6;
                    cp |= 0x4000000;
                    goto case 5;
                case 5:
                    str[4] = (char)(0x80 | (cp & 0x3f));
                    cp >>= 6;
                    cp |= 0x200000;
                    goto case 4;
                case 4:
                    str[3] = (char)(0x80 | (cp & 0x3f));
                    cp >>= 6;
                    cp |= 0x10000;
                    goto case 3;
                case 3:
                    str[2] = (char)(0x80 | (cp & 0x3f));
                    cp >>= 6;
                    cp |= 0x800;
                    goto case 2;
                case 2:
                    str[1] = (char)(0x80 | (cp & 0x3f));
                    cp >>= 6;
                    cp |= 0xc0;
                    goto case 1;
                case 1:
                    str[0] = (char)cp;
                    break;
            }

            string result = "";
            for (ushort i = 0; str[i] != '\0'; i++)
            {
                result += str[i];
            }
            return result;
        }

        private static bool IsBlack(Colour col)
        {
            return col.R == 0.0f && col.G == 0.0f && col.B == 0.0f && col.A == 0.0f;
        }

    }
}
