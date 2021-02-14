namespace SilkyNvg.Core
{
    public struct LaunchParameters
    {

        public bool EdgeAntialias { get; private set; }
        public bool StencilStrokes { get; private set; }
        public bool Debug { get; set; }

        public LaunchParameters(bool edgeAA, bool stencilStrokes, bool debug)
        {
            EdgeAntialias = edgeAA;
            StencilStrokes = stencilStrokes;
            Debug = debug;
        }

    }
}
