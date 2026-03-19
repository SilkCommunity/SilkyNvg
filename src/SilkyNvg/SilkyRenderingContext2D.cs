using System;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public class SilkyRenderingContext2D : IDisposable
    {

        private readonly RenderTolerances _tolerances = new RenderTolerances();

        private readonly ISilkyRenderer _renderer;

        private readonly Scene _scene;
        
        public SilkyRenderingContext2D(ISilkyRenderer renderer)
        {
            _renderer = renderer;

            _scene = new Scene();
        }
        
        #region SilkyContentArea
        
        public int Width { get; private set; }
        
        public int Height { get; private set; }
      
        public float PixelRatio { get; private set; }

        public void Resize(int width, int height, float pixelRatio)
        {
            Width = width;
            Height = height;
            PixelRatio = pixelRatio;

            _renderer.Resize(width, height, pixelRatio);
        }
        
        #endregion
        
        #region SilkyRendering

        public void BeginFrame()
        {
            _scene.Clear();
        }

        public void EndFrame()
        {
            _renderer.AddScene(_scene);
            _renderer.Render();
        }
        
        #endregion
        
        #region SilkyDrawPath

        public void Fill(Path2D path)
        {
            path.FillToScene(_scene, _tolerances);
        }
        
        #endregion

        public void Dispose()
        {
            
        }
    }
}