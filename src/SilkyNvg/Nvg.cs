using SilkyNvg.Common;
using SilkyNvg.Rendering;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        public static Nvg Create(CreateFlags flags, INvgRenderer renderer)
        {
            LaunchParameters launchParameters = new(
                (flags & CreateFlags.Antialias) != 0,
                (flags & CreateFlags.StencilStrokes) != 0,
                (flags & CreateFlags.Debug) != 0
            );

            GraphicsManager graphicsManager = new(launchParameters, renderer);

            Nvg nvg = new(launchParameters, graphicsManager);
            return nvg;
        }

        internal readonly LaunchParameters launchParameters;
        internal readonly GraphicsManager graphicsManager;

        private Nvg(LaunchParameters launchParameters, GraphicsManager graphicsManager)
        {
            this.launchParameters = launchParameters;
            this.graphicsManager = graphicsManager;

            Initialize();
        }

        private void Initialize()
        {

        }

    }
}
