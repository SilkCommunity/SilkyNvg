using Silk.NET.OpenGL;
using SilkyNvg.Blending;
using SilkyNvg.Colouring;
using SilkyNvg.Common;
using System.Collections.Generic;

namespace SilkyNvg.OpenGL
{
    internal sealed class GraphicsManager
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

        public void Create()
        {
            _interface = new GLInterface(this);
        }

        public void Viewport(float width, float height)
        {
            _interface.Viewport(width, height);
        }

        public void  Flush()
        {
            _interface.Flush();
        }

        public void Fill(Paint paint, CompositeOperationState compositeOperation, Scissor scissor,
            float fringe, float[] bounds, List<Core.Paths.Path> paths)
        {
            _interface.Fill(paint, compositeOperation, scissor, fringe, bounds, paths.ToArray());
        }

    }
}