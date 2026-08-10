using SilkyNvg.Paths;

namespace SilkyNvg
{
    public class SilkyRenderingContext2D
    {

        private readonly Path2D _defaultPath;
        
        #region Meta
        
        public uint Width { get; set; }
        
        public uint Height { get; set; }
        
        #endregion
        
        public SilkyRenderingContext2D(uint width, uint height)
        {
            Width = width;
            Height = height;

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
            
        }
        
        #endregion
        
    }
}