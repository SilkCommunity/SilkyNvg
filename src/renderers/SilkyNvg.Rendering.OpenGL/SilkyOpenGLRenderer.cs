using System;
using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL
{
    public sealed class SilkyOpenGLRenderer : ISilkyRenderer
    {

        private readonly GL _gl;

        private RenderTolerances _tolerances;

        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;
        }

        public void Init(RenderTolerances tolerances)
        {
            _tolerances = tolerances;
        }

        public void Resize(float width, float height, float pixelRatio)
        {
            
        }

        public void AddScene(Scene scene)
        {
            
        }

        public void Render()
        {
            
        }
            
        public void Dispose()
        {
            
        }
        
    }
}