using SilkyNvg.Paths;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public class SilkyRenderingContext2D
    {

        private readonly ISilkyRenderer _renderer;
        
        private readonly Path2D _defaultPath;
        private readonly GeometryBuilder _geometryBuilder;

        private uint _width;
        private uint _height;
        
        #region Meta

        public uint Width
        {
            get => _width;
            set => Resize(value, _height);
        }

        public uint Height
        {
            get => _height;
            set => Resize(_width, value);
        }

        public void Resize(uint newWidth, uint newHeight)
        {
            _width = newWidth;
            _height = newHeight;
            _renderer.Resize(newWidth, newHeight);
        }
        
        #endregion
        
        public SilkyRenderingContext2D(uint initialWidth, uint initialHeight, ISilkyRenderer renderer)
        {
            _renderer = renderer;
            Resize(initialWidth, initialHeight);
            
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