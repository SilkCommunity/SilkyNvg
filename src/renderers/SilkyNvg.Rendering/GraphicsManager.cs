using SilkyNvg.Common;
using System.Numerics;

namespace SilkyNvg.Rendering
{
    internal class GraphicsManager
    {

        private readonly LaunchParameters _launchParameters;
        private readonly INvgRenderer _renderer;

        public LaunchParameters LaunchParameters => _launchParameters;

        public INvgRenderer Renderer => _renderer;

        public GraphicsManager(LaunchParameters launchParameters, INvgRenderer renderer)
        {
            _launchParameters = launchParameters;
            _renderer = renderer;
        }

        public bool Create()
        {
            return _renderer.Create(_launchParameters);
        }

        public void Viewport(Vector2 viewSize, float devicePxRatio)
        {
            _renderer.Viewport(viewSize, devicePxRatio);
        }

    }
}
