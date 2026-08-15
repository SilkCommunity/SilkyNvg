using SilkyNvg.Paths;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public class SilkyRenderingContext2D
    {

        private readonly ISilkyRenderer _renderer;
        
        private readonly Path2D _defaultPath;
        
        #region Meta
        
        public uint Width { get; set; }
        
        public uint Height { get; set; }
        
        #endregion
        
        public SilkyRenderingContext2D(uint width, uint height, ISilkyRenderer renderer)
        {
            Width = width;
            Height = height;

            _renderer = renderer;
            
            _defaultPath = new Path2D();
        }
        
        #region DrawPath

        public void BeginPath()
        {
            _defaultPath.ClearSubPaths();
        }

        public void Fill(Path2D path)
        {
            // TODO: Apply Transform
            path.RenderPath(_renderer);
        }
        
        #endregion
        
    }
}