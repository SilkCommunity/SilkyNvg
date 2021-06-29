using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Text;
using System;

namespace NvgExample
{

    public class PerformanceGraph
    {

        private const uint GRAPH_HISTORY_COUNT = 100;

        public enum GraphRenderStyle
        {
            Fps,
            Ms,
            Percent
        }

        private readonly GraphRenderStyle _style;
        private readonly string _name;
        private readonly float[] _values = new float[GRAPH_HISTORY_COUNT];

        private uint _head;

        public float GraphAverage
        {
            get
            {
                float avg = 0;
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    avg += _values[i];
                }
                return avg / (float)GRAPH_HISTORY_COUNT;
            }
        }

        public PerformanceGraph(GraphRenderStyle style, string name)
        {
            _style = style;
            _name = name;
        }

        public void Update(float frameTime)
        {
            _head = (_head + 1) % GRAPH_HISTORY_COUNT;
            _values[_head] = frameTime;
        }

        public void Render(float x, float y, Nvg nvg)
        {
            float avg = GraphAverage;

            float w = 200.0f;
            float h = 35.0f;

            nvg.BeginPath();
            nvg.Rect(x, y, w, h);
            nvg.FillColour(nvg.Rgba(0, 0, 0, 128));
            nvg.Fill();

            nvg.BeginPath();
            nvg.MoveTo(x, y + h);
            if (_style == GraphRenderStyle.Fps)
            {
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = 1.0f / (0.00001f + _values[(_head + i) % GRAPH_HISTORY_COUNT]);
                    v = MathF.Min(v, 80.0f);
                    float vx = ((float)i / (GRAPH_HISTORY_COUNT - 1)) * w;
                    float vy = y + h - ((v / 80.0f) * h);
                    nvg.LineTo(vx, vy);
                }
            }
            else if (_style == GraphRenderStyle.Percent)
            {
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = _values[(_head + i) % GRAPH_HISTORY_COUNT] * 1.0f;
                    v = MathF.Min(v, 100.0f);
                    float vx = x + ((float)i / (GRAPH_HISTORY_COUNT - 1)) * w;
                    float vy = y + h - ((v / 100.0f) * h);
                    nvg.LineTo(vx, vy);
                }
            }
            else if (_style == GraphRenderStyle.Ms)
            {
                for (uint i = 0; i < GRAPH_HISTORY_COUNT; i++)
                {
                    float v = _values[(_head + i) % GRAPH_HISTORY_COUNT] * 1000.0f;
                    v = MathF.Min(v, 20.0f);
                    float vx = x + ((float)i / (GRAPH_HISTORY_COUNT - 1)) * w;
                    float vy = y + h - ((v / 20.0f) * h);
                    nvg.LineTo(vx, vy);
                }
            }

            nvg.LineTo(x + w, y + h);
            nvg.FillColour(nvg.Rgba(255, 192, 0, 128));
            nvg.Fill();

            nvg.FontFace("sans");

            if (_name != null)
            {
                nvg.FontSize(12.0f);
                nvg.TextAlign(Align.Left | Align.Top);
                nvg.FillColour(nvg.Rgba(240, 240, 240, 192));
                _ = nvg.Text(x + 3.0f, y + 3.0f, _name);
            }

            if (_style == GraphRenderStyle.Fps)
            {
                nvg.FontSize(15.0f);
                nvg.TextAlign(Align.Right | Align.Top);
                nvg.FillColour(nvg.Rgba(240, 240, 240, 160));
                float val = 1.0f / avg;
                string str = (float.IsInfinity(val) ? "inf" : val) + " FPS";
                _ = nvg.Text(x + w - 3.0f, y + 3.0f, str);

                nvg.FontSize(13.0f);
                nvg.TextAlign(Align.Right | Align.Baseline);
                nvg.FillColour(nvg.Rgba(240, 240, 240, 160));
                val = avg * 1000.0f;
                str = val + " ms";
                _ = nvg.Text(x + w - 3.0f, y + h - 3.0f, str);
            }
            else if (_style == GraphRenderStyle.Percent)
            {
                nvg.FontSize(15.0f);
                nvg.TextAlign(Align.Right | Align.Top);
                nvg.FillColour(nvg.Rgba(240, 240, 240, 255));
                string str = avg * 1.0f + "%";
                _ = nvg.Text(x + w - 3.0f, y + 3.0f, str);
            }
            else if (_style == GraphRenderStyle.Ms)
            {
                nvg.FontSize(15.0f);
                nvg.TextAlign(Align.Right | Align.Top);
                nvg.FillColour(nvg.Rgba(240, 240, 240, 255));
                string str = avg * 1000.0f + " ms";
                _ = nvg.Text(x + w - 3.0f, y + 3.0f, str);
            }
        }

    }
}
