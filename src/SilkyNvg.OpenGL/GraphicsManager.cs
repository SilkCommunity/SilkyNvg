using Silk.NET.OpenGL;
using SilkyNvg.Core;

namespace SilkyNvg.OpenGL
{
    public sealed class GraphicsManager
    {

        private readonly LaunchParameters _launchParameters;
        private readonly GL _gl;

        public GraphicsManager(LaunchParameters launchParameters, GL gl)
        {
            _launchParameters = launchParameters;
            _gl = gl;
        }

    }
}
