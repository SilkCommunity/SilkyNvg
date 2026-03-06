using Silk.NET.OpenGL;
using SilkyNvg.Rendering;

namespace OpenGLRenderer
{
    public sealed class OpenGLRenderer : ISilkyRenderer
    {

        private readonly GL _gl;

        public OpenGLRenderer(GL gl)
        {
            _gl = gl;
        }

    }
}