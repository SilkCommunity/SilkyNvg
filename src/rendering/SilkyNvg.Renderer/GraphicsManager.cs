using Silk.NET.Maths;
using SilkyNvg.Blending;
using System;

namespace SilkyNvg.Renderer
{
    internal class GraphicsManager
    {

        private INvgRenderer _renderer;

        public bool EdgeAntiAlias => _renderer.EdgeAntiAlias;

        public GraphicsManager(INvgRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException("Cannot initialize SilkyNvg without a renderer!");
        }

        public bool Create()
        {
            return _renderer.Create();
        }

        public void Viewport(Vector2D<float> size, float devicePixelRatio)
        {
            _renderer.Viewport(size, devicePixelRatio);
        }

        public void Flush()
        {
            _renderer.Flush();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor, float fringeWidth, Vector4D<float> bounds, Path[] paths)
        {
            _renderer.Fill(paint, compositeOperation, scissor, fringeWidth, bounds, paths);
        }

        public void Stroke(Paint strokePaint, CompositeOperationState compositeOperation, Scissor scissor, float fringeWidth, float strokeWidth, Path[] paths)
        {
            _renderer.Stroke(strokePaint, compositeOperation, scissor, fringeWidth, strokeWidth, paths);
        }

    }
}
