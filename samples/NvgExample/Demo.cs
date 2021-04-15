using Silk.NET.OpenGL;
using SilkyNvg;
using System;

namespace NvgExample
{
    public class Demo
    {

        private readonly Nvg _nvg;
        private readonly GL _gl;

        public Demo(Nvg nvg, GL gl)
        {
            _nvg = nvg;
            _gl = gl;
            // TODO FontsNStuff
        }

        private void DrawEyes(float x, float y, float w, float h, float mx, float my, float t)
        {
            float ex = w * 0.23f;
            float ey = h * 0.5f;
            float lx = x + ex;
            float ly = y + ey;
            float rx = x + w - ex;
            float ry = y + ey;
            float br = (ex < ey ? ex : ey) * 0.5f;
            float blink = 1 - MathF.Pow(MathF.Sin(t * 0.5f), 200.0f) * 0.8f;

            Paint bg = Paint.LinearGradient(x, y + h * 0.5f, x + w * 0.1f, y + h, new Colour(0, 0, 0, 32), new Colour(0, 0, 0, 16));
            _nvg.BeginPath();
            _nvg.Ellipse(lx + 3.0f, ly + 16.0f, ex, ey);
            _nvg.Ellipse(rx + 3.0f, ry + 16.0f, ex, ey);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            bg = Paint.LinearGradient(x, y + h * 0.25f, x + w * 0.1f, y + h, new Colour(220, 220, 200, 255), new Colour(128, 128, 128, 255));
            _nvg.BeginPath();
            _nvg.Ellipse(lx, ly, ex, ey);
            _nvg.Ellipse(rx, ry, ex, ey);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            float dx = (mx - rx) / (ex * 10);
            float dy = (my - ry) / (ey * 10);
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }
            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            _nvg.BeginPath();
            _nvg.Ellipse(lx + dx, ly + dy + ey * 0.25f * (1 - blink), br, br * blink);
            _nvg.FillColour(new Colour(32, 32, 32, 255));
            _nvg.Fill();

            dx = (mx - rx) / (ex * 10);
            dy = (my - ry) / (ey * 10);
            d = MathF.Sqrt(dx * dx + dy * dy);
            if (d > 1.0f)
            {
                dx /= d;
                dy /= d;
            }
            dx *= ex * 0.4f;
            dy *= ey * 0.5f;
            _nvg.BeginPath();
            _nvg.Ellipse(rx + dx, ry + dy + ey * 0.25f * (1 - blink), br, br * blink);
            _nvg.FillColour(new Colour(32, 32, 32, 255));
            _nvg.Fill();

            Paint gloss = Paint.RadialGradient(lx - ex * 0.25f, ly - ey * 0.5f, ex * 0.1f, ex * 0.75f, new Colour(255, 255, 255, 128), new Colour(255, 255, 255, 0));
            _nvg.BeginPath();
            _nvg.Ellipse(lx, ly, ex, ey);
            _nvg.FillPaint(gloss);
            _nvg.Fill();

            gloss = Paint.RadialGradient(rx - ex * 0.25f, ry - ey * 0.5f, ex * 0.1f, ex * 0.75f, new Colour(255, 255, 255, 128), new Colour(255, 255, 255, 0));
            _nvg.BeginPath();
            _nvg.Ellipse(rx, ry, ex, ey);
            _nvg.FillPaint(gloss);
            _nvg.Fill();
        }

        private void DrawGraph(float x, float y, float w, float h, float t)
        {
            float dx = w / 5.0f;

            float[] samples =
            {
                (1 + MathF.Sin(t * 1.2345f + MathF.Cos(t * 0.33457f) * 0.44f)) * 0.5f,
                (1 + MathF.Sin(t * 0.68363f + MathF.Cos(t * 1.3f) * 1.55f)) * 0.5f,
                (1 + MathF.Sin(t * 1.1642f + MathF.Cos(t * 0.33457f) * 1.24f)) * 0.5f,
                (1 + MathF.Sin(t * 0.56345f + MathF.Cos(t * 1.63f) * 0.14f)) * 0.5f,
                (1 + MathF.Sin(t * 1.6245f + MathF.Cos(t * 0.254f) * 0.3f)) * 0.5f,
                (1 + MathF.Sin(t * 0.345f + MathF.Cos(t * 0.03f) * 0.6f)) * 0.5f
            };

            float[] sx = new float[6];
            float[] sy = new float[6];

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

        private void DrawColourWheel(float x, float y, float w, float h, float t)
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
                _nvg.Arc(cx, cy, r0, a0, a1, Winding.CW);
                _nvg.Arc(cx, cy, r1, a1, a0, Winding.CCW);
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
            _nvg.PathWinding((int)Solidity.Hole);
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
            _nvg.PathWinding((int)Solidity.Hole);
            _nvg.FillPaint(paint);
            _nvg.Fill();

            _nvg.Restore();
            _nvg.Restore();

        }

        private void DrawLines(float x, float y, float w, float h, float t)
        {
            float pad = 5.0f;
            float s = w / 9.0f - pad * 2;
            float[] pts = new float[4 * 2];
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

                    _nvg.SetLineCap(caps[i]);
                    _nvg.SetLineJoin(joins[j]);

                    _nvg.StrokeWidth(s * 0.3f);
                    _nvg.StrokeColour(new Colour(0, 0, 0, 160));
                    _nvg.BeginPath();
                    _nvg.MoveTo(fx + pts[0], fy + pts[1]);
                    _nvg.LineTo(fx + pts[2], fy + pts[3]);
                    _nvg.LineTo(fx + pts[4], fy + pts[5]);
                    _nvg.LineTo(fx + pts[6], fy + pts[7]);
                    _nvg.Stroke();

                    _nvg.SetLineCap(LineCap.Butt);
                    _nvg.SetLineJoin(LineCap.Bevel);

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

        private void DrawWidths(float x, float y, float width)
        {
            _nvg.Save();

            _nvg.StrokeColour(new Colour(0, 0, 0, 255));

            for (int i = 0; i < 20; i++)
            {
                float w = ((float)i + 0.5f) * 0.1f;
                _nvg.StrokeWidth(w);
                _nvg.BeginPath();
                _nvg.MoveTo(x, y);
                _nvg.LineTo(x + width, y + width * 0.3f);
                _nvg.Stroke();
                y += 10;
            }

            _nvg.Restore();
        }

        private void DrawCaps(float x, float y, float width)
        {
            LineCap[] caps = { LineCap.Butt, LineCap.Round, LineCap.Square };
            float lineWidth = 8.0f;

            _nvg.Save();

            _nvg.BeginPath();
            _nvg.Rect(x - lineWidth / 2, y, width + lineWidth, 40);
            _nvg.FillColour(new Colour(255, 255, 255, 32));
            _nvg.Fill();

            _nvg.BeginPath();
            _nvg.Rect(x, y, width, 40);
            _nvg.FillColour(new Colour(255, 255, 255, 32));
            _nvg.Fill();

            _nvg.StrokeWidth(lineWidth);
            for (int i = 0; i < 3; i++)
            {
                _nvg.SetLineCap(caps[i]);
                _nvg.StrokeColour(new Colour(0, 0, 0));
                _nvg.BeginPath();
                _nvg.MoveTo(x, y + i * 10 + 5);
                _nvg.LineTo(x + width, y + i * 10 + 5);
                _nvg.Stroke();
            }

            _nvg.Restore();
        }

        private void DrawScissor(float x, float y, float t)
        {
            _nvg.Save();

            _nvg.Translate(x, y);
            _nvg.Rotate(_nvg.DegToRad(5));
            _nvg.BeginPath();
            _nvg.Rect(-20, -20, 60, 40);
            _nvg.FillColour(new Colour(255, 0, 0));
            _nvg.Fill();
            _nvg.Scissor(-20, -20, 60, 40);

            _nvg.Translate(40, 0);
            _nvg.Rotate(t);

            _nvg.Save();
            _nvg.ResetScissor();
            _nvg.BeginPath();
            _nvg.Rect(-20, -10, 60, 30);
            _nvg.FillColour(_nvg.Rgba(255, 128, 0, 64));
            _nvg.Fill();
            _nvg.Restore();

            _nvg.IntersectScissor(-20, -10, 60, 30);
            _nvg.BeginPath();
            _nvg.Rect(-20, -10, 60, 30);
            _nvg.FillColour(new Colour(255, 128, 0, 255));
            _nvg.Fill();

            _nvg.Restore();
        }

        public void Render(float mx, float my, float width, float height, float t, bool blowup)
        {
            DrawEyes(width - 250, 50, 150, 100, mx, my, t);
            // TODO: Draw paragraph
            DrawGraph(0, height / 2, width, height / 2, t);
            DrawColourWheel(width - 300, height - 300, 250.0f, 250.0f, t);

            DrawLines(120, height - 50, 600, 50, t);

            DrawWidths(10, 50, 30);

            DrawCaps(10, 300, 30);

            DrawScissor(50, height - 80, t); // FIXME

            _nvg.Save();
            if (blowup)
            {
                _nvg.Rotate(MathF.Sin(t * 0.3f) * 5.0f / 180.0f * MathF.PI);
                _nvg.Scale(2.0f, 2.0f);
            }
        }

    }
}
