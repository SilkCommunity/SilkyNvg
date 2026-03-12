using System;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public sealed class SilkyRenderingContext : IDisposable
    {

        private readonly RenderTolerances _renderTolerances = new RenderTolerances();
        
        private readonly ISilkyRenderer _renderer;

        private float _width, _height;
        private float _pixelRatio;

        public SilkyRenderingContext(ISilkyRenderer renderer)
        {
            _renderer = renderer;

            Resize(1.0f, 1.0f, 1.0f);
        }

        public float Width
        {
            get => _width;
            set => Resize(value, _height, _pixelRatio);
        }

        public float Height
        {
            get => _height;
            set => Resize(_width, value, _pixelRatio);
        }

        public float PixelRatio
        {
            get => _pixelRatio;
            set => Resize(_width, _height, value);
        }

        public void Resize(float width, float height, float pixelRatio)
        {
            _width = width;
            _height = height;
            _pixelRatio = pixelRatio;
            _renderTolerances.Update(_pixelRatio);
        }

        public void FillPath(Path path)
        {
            
        }

        public void Dispose()
        {
            
        }
        
    }
}