using Silk.NET.OpenGL;
using SilkyNvg.Rendering;

namespace SilkyNvg.Renderers.OpenGL
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly GL _gl;

        public OpenGLRenderer(GL gl)
        {
            _gl = gl;
        }

        public void Dispose()
        {
            
        }
        
    }
}