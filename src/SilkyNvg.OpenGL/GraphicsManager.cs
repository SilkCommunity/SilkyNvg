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
        public LaunchParameters LaunchParameters => _launchParameters;

        internal GL GL => _gl;

        public GraphicsManager(LaunchParameters launchParameters, GL gl)
        {
            _launchParameters = launchParameters;
            _gl = gl;
        }

        public void CreateRenderer()
        {
            _interface = new GLInterface(this);
        }

        public void SetViewport(float width, float height)
        {
            _interface.SetupViewSize(width, height);
        }

        public void RenderFill(Paint paint, Core.Composite.ICompositeOperation compositeOperation, Scissor scissor,
            float fringe, Silk.NET.Maths.Rectangle<float> bounds, Core.Paths.Path[] paths, int pathCount)
        {
            _interface.RenderFill(paint, compositeOperation, scissor, fringe, bounds, paths, pathCount);
        }

        public void RenderFlush()
        {
            _interface.RenderFlush();
        }

    }
}
