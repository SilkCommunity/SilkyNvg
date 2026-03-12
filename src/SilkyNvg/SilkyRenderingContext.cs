using System;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public sealed class SilkyRenderingContext : IDisposable
    {

        private readonly RenderTolerances _renderTolerances = new RenderTolerances();
        
        private readonly ISilkyRenderer _renderer;
        
        private readonly FrameContainer _frameContainer;

        private float _width, _height;
        private float _pixelRatio;

        public SilkyRenderingContext(ISilkyRenderer renderer)
        {
            _renderer = renderer;

            _frameContainer = new FrameContainer(_renderTolerances);

            Resize(1.0f, 1.0f, 1.0f);
        }

        #region Viewport
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
        #endregion

        public void BeginFrame()
        {
            _frameContainer.Clear();
        }

        public void EndFrame()
        {
            
        }

        public void FillPath(Path path)
        {
            path.Fill(_frameContainer);
        }

        public void Dispose()
        {
            
        }
        
    }
}