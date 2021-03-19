using Silk.NET.OpenGL;
using SilkyNvg;
using SilkyNvg.Colouring;
using SilkyNvg.Paths;
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
            float blink = 1 - MathF.Pow(MathF.Sin(t * 0.5f), 200) * 0.8f;

            Paint bg = Paint.LinearGradient(x, y + h * 0.5f, x + w * 0.1f, y + h, new Colour(0, 0, 0, 32), new Colour(0, 0, 0, 16));
            _nvg.BeginPath();
            _nvg.Ellipse(lx + 3.0f, ly + 16.0f, ex, ey);
            _nvg.Ellipse(rx + 3.0f, ry + 16.0f, ex, ey);
            _nvg.FillPaint(bg);
            _nvg.Fill();

            bg = Paint.LinearGradient(x, y + h * 0.25f, x + w * 0.1f, y + h, new Colour(220, 220, 220, 255), new Colour(128, 128, 128, 255));
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
            float[] samples =
            {
                (1 + MathF.Sin(t * 1.2345f + MathF.Cos(t * 0.3457f) * 0.44f)) * 0.5f,
                (1 + MathF.Sin(t * 0.68363f + MathF.Cos(t * 1.3f) * 1.55f)) * 0.5f,
                (1 + MathF.Sin(t * 1.1642f + MathF.Cos(t * 0.33457f) * 1.24f)) * 0.5f,
                (1 + MathF.Sin(t * 0.56345f + MathF.Cos(t * 1.63f) * 1.14f)) * 0.5f,
                (1 + MathF.Sin(t * 1.6245f + MathF.Cos(t * 0.254f) * 0.3f)) * 0.5f,
                (1 + MathF.Sin(t * 0.345f + MathF.Cos(t * 0.03f) * 0.6f)) * 0.5f,
            };

            float[] sx = new float[6];
            float[] sy = new float[6];
            float dx = w / 5.0f;

            for (int i = 0; i < 6; i++)
            {
                sx[i] = x + i * dx;
                sy[i] = y + h * samples[i] * 0.8f;
            }

            Paint bg = Paint.LinearGradient(x, y, x + h, y + h, new Colour(0, 160, 192, 0), new Colour(0, 160, 192, 64));
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
            _nvg.MoveTo(sx[0], sy[0]);
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
                bg = Paint.RadialGradient(sy[i], sy[i] + 2, 3.0f, 8.0f, new Colour(0, 0, 0, 32), new Colour(0, 0, 0, 0));
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

            _nvg.StrokeWidth(1.0f);
        }

        private void DrawColourWheel(float x, float y, float w, float h, float t)
        {
            _nvg.Save();

            float cx = x + w * 0.5f;
            float cy = y + h * 0.5f;
            float r1 = (w < h ? w : h) * 0.5f - 5.0f;
            float r0 = r1 - 20.0f;
            float aeps = 0.5f / r1;

            for (int i = 0; i < 6; i++)
            {
                float a0 = (float)i / 6.0f * MathF.PI * 2.0f - aeps;
                float a1 = (float)(i + 1.0f) / 6.0f * MathF.PI * 2.0f - aeps;
                _nvg.BeginPath();
                _nvg.Arc(cx, cy, r0, a0, a1, Winding.CW);
                _nvg.Arc(cx, cy, r1, a1, a0, Winding.CCW);
                _nvg.ClosePath();
                float ax = cx + MathF.Cos(a0) * (r0 + r1) * 0.5f;
                float ay = cy + MathF.Sin(a0) * (r0 + r1) * 0.5f;
                float bx = cx + MathF.Cos(a1) * (r0 + r1) * 0.5f;
                float by = cy + MathF.Sin(a1) * (r0 + r1) * 0.5f;
                var paint = _nvg.LinearGradient(ax, ay, bx, by, _nvg.Hsla(a0 / (MathF.PI * 2), 1.0f, 0.55f, 255), _nvg.Hsla(a1 / (MathF.PI * 2), 1.0f, 0.55f, 255));
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

        }

        public void Render(float mx, float my, float width, float height, float t, bool blowup)
        {
            DrawEyes(width - 250, 50, 150, 100, mx, my, t);
            // Draw Paragraph
            DrawGraph(0, height / 2, width, height / 2, t);
            DrawColourWheel(width - 300, height - 300, 250.0f, 250.0f, t);
        }

    }
}
