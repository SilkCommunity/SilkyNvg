using SilkyNvg.Core;
using SilkyNvg.Core.Paths;
using SilkyNvg.OpenGL;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        private readonly GraphicsManager _graphicsManager;
        private readonly PathCache _pathCache;

        private Nvg(GraphicsManager graphicsManager)
        {
            _graphicsManager = graphicsManager;

            // TODO: Images

            // TODO Commands

            _pathCache = new PathCache();
        }

        /// <summary>
        /// Creates a new instance of the NanoVG API. Each Nvg-API has its own
        /// context, so instead of needing to parse it in all the time, one creates
        /// a new API-Instance. An API instance will also be refered to as "context"
        /// in the following documentation, when it is more applicable.
        /// </summary>
        /// <param name="flags">The flags to be used when this context is running <see cref="CreateFlag"/>.</param>
        /// <param name="gl">The GL Api object needed for rendering.</param>
        public static Nvg Create(uint flags, Silk.NET.OpenGL.GL gl)
        {
            var launchParams = new LaunchParameters(flags);
            var gManager = new GraphicsManager(launchParams, gl);
            var nvg = new Nvg(gManager);
            return nvg;
        }

    }
}
