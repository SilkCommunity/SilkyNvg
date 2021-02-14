using Silk.NET.OpenGL;

namespace SilkyNvg.OpenGL
{
    public sealed class GraphicsManager
    {

        private readonly uint _flags;
        private readonly bool _edgeAntialiasing;
        private readonly GL _gl;

        public GraphicsManager(uint flags, bool edgeAntiAliasing, GL gl)
        {
            _flags = flags;
            _edgeAntialiasing = edgeAntiAliasing;
            _gl = gl;
        }

    }
}
