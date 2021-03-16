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
            float blink = 1 - MathF.Pow(MathF.Sin(t * 0.5f), 200) * 0.8f;


        }

        public void Render()
        {

        }

    }
}
