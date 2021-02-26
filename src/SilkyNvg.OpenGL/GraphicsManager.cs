using Silk.NET.OpenGL;
using SilkyNvg.Core;

namespace SilkyNvg.OpenGL
{
    public sealed class GraphicsManager
    {

        private readonly LaunchParameters _launchParameters;
        private readonly GL _gl;

        private GLInterface _interface;

        public GLInterface GraphicsInterface => _interface;

        internal GL GL => _gl;
        internal LaunchParameters LaunchParameters => _launchParameters;

        public GraphicsManager(LaunchParameters launchParameters, GL gl)
        {
            _launchParameters = launchParameters;
            _gl = gl;
        }

        public void RenderCreate()
        {
            _interface = new GLInterface(this);
        }

        public void RenderViewport(float width, float height)
        {
            _interface.SetupViewSize(width, height);
        }

    }
}
