using Silk.NET.OpenGL;
using SilkyNvg.Core;
using SilkyNvg.Core.Paths;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL
{
    public sealed class GraphicsManager
    {

        private readonly LaunchParameters _launchParameters;
        private readonly GL _gl;

        private GLInterface _interface;

        internal GLInterface GraphicsInterface => _interface;

        internal GL GL => _gl;
        public LaunchParameters LaunchParameters => _launchParameters;

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
            _interface.RenderViewport(width, height);
        }

        public void RenderFlush()
        {
            _interface.RenderFlush();
        }

        public void RenderFill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor,
            float fringe, Silk.NET.Maths.Rectangle<float> bounds, List<SilkyNvg.Core.Paths.Path> paths)
        {
            _interface.RenderFill(paint, compositeOperation, scissor, fringe, bounds, paths.ToArray());
        }

    }
}
