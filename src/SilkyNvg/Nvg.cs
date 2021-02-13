using SilkyNvg.OpenGL;

namespace SilkyNvg
{
    public sealed class Nvg
    {

        private Nvg(GraphicsManager graphicsManager, bool edgeAntialiased)
        {
            System.Console.WriteLine(edgeAntialiased);
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
            var gManager = new GraphicsManager(flags);
            bool edgeAA = (flags & (uint)CreateFlag.EdgeAntialias) != 0;
            var nvg = new Nvg(gManager, edgeAA);
            return nvg;
        }

    }
}
