using SilkyNvg.Common;

namespace SilkyNvg.Rendering
{
    internal class GraphicsManager
    {

        private readonly LaunchParameters _launchParameters;
        private readonly INvgRenderer _renderer;

        public GraphicsManager(LaunchParameters launchParameters, INvgRenderer renderer)
        {
            _launchParameters = launchParameters;
            _renderer = renderer;
        }

    }
}
