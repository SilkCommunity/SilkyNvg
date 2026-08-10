using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL
{
    public class SilkyOpenGLRenderer : ISilkyRenderer
    {

        private readonly GL _gl;

        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;
        }

        public void Dispose()
        {
            
        }
        
    }
}