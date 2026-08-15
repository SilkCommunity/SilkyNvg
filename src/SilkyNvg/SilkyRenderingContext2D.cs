using SilkyNvg.Paths;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public class SilkyRenderingContext2D
    {

        private readonly ISilkyRenderer _renderer;
        
        private readonly Path2D _defaultPath;
        private readonly GeometryBuilder _geometryBuilder;
        
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
            _geometryBuilder = new GeometryBuilder();
        }

        public void BeginFrame()
        {
            _geometryBuilder.ClearGeometry();
        }

        public void EndFrame()
        {
            _renderer.Render(_geometryBuilder.VertexData, _geometryBuilder.VertexCount, _geometryBuilder.Paths);
        }
        
        #region DrawPath

        public void BeginPath()
        {
            _defaultPath.ClearSubPaths();
        }

        public void Fill(Path2D path)
        {
            // TODO: Apply Transform
            path.FillPath(_geometryBuilder);
        }
        
        #endregion
        
    }
}