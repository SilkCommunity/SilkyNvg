using Silk.NET.Maths;
using SilkyNvg.Blending;
using SilkyNvg.Images;
using System;

namespace SilkyNvg.Rendering
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

        public int CreateTexture(Texture type, Vector2D<uint> size, ImageFlags imageFlags, byte[] data)
        {
            return _renderer.CreateTexture(type, size, imageFlags, data);
        }

        public bool DeleteTexture(int image)
        {
            return _renderer.DeleteTexture(image);
        }

        public bool UpdateTexture(int image, Vector4D<uint> bounds, byte[] data)
        {
            return _renderer.UpdateTexture(image, bounds, data);
        }

        public bool GetTextureSize(int image, out Vector2D<uint> bounds)
        {
            return _renderer.GetTextureSize(image, out bounds);
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

        public void Delete()
        {
            _renderer.Dispose();
        }

    }
}
