using Silk.NET.OpenGL;

namespace SilkyNvg.Rendering.OpenGL
{
    public class SilkyOpenGLRenderer : ISilkyRenderer
    {

        private readonly SceneContainer _scene;
        private readonly GL _gl;

        public ISceneContainer SceneContainer => _scene;

        public SilkyOpenGLRenderer(GL gl)
        {
            _gl = gl;

            _scene = new SceneContainer(3, gl);
        }

        public void Resize(uint width, uint height, RenderTolerances _)
        {
            
        }

        public void BeginFrame()
        {
            _scene.BeginFrame();
        }

        public void EndFrame()
        {
            _scene.EndFrame();

            _scene.Clear();
        }

        public void Dispose()
        {
            _scene.Dispose();
        }
        
    }
}