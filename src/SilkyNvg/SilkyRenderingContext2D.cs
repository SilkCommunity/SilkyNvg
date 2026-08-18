using SilkyNvg.Paths;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public class SilkyRenderingContext2D
    {

        private readonly ISilkyRenderer _renderer;
        
        private readonly Path2D _defaultPath;
        private readonly GeometryBuilder _geometryBuilder;
        private readonly RenderTolerances _tolerances;

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

        public void Resize(uint newWidth, uint newHeight, float newPixelRatio = 1.0f)
        {
            _width = newWidth;
            _height = newHeight;
            _tolerances.Update(newPixelRatio);
            _renderer.Resize(newWidth, newHeight, _tolerances);
        }
        
        #endregion
        
        public SilkyRenderingContext2D(ISilkyRenderer renderer, uint initialWidth, uint initialHeight, float initialPixelRatio = 1.0f)
        {
            _renderer = renderer;
            
            _defaultPath = new Path2D();
            _tolerances = new RenderTolerances();
            _geometryBuilder = new GeometryBuilder(_tolerances);
            
            Resize(initialWidth, initialHeight, initialPixelRatio);
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