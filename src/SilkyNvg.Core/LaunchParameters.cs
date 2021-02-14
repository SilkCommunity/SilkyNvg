namespace SilkyNvg.Core
{
    public struct LaunchParameters
    {

        public bool EdgeAntialias { get; private set; }
        public bool StencilStrokes { get; private set; }
        public bool Debug { get; set; }

        public LaunchParameters(uint flags)
        {
            EdgeAntialias = (flags & (uint)CreateFlag.EdgeAntialias) != 0;
            StencilStrokes = (flags & (uint)CreateFlag.StencilStrokes) != 0;
            Debug = (flags & (uint)CreateFlag.Debug) != 0;
        }

    }
}
