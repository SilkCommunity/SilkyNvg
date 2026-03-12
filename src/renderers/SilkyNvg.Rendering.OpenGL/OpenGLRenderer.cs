using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL
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