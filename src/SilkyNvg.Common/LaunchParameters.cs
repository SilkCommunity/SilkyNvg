namespace SilkyNvg.Common
{
    internal struct LaunchParameters
    {

        private readonly bool _antialias;
        private readonly bool _stencilStrokes;
        private readonly bool _debug;

        public bool Antialias => _antialias;
        public bool StencilStrokes => _stencilStrokes;
        public bool Debug => _debug;

        public LaunchParameters(bool edgeAA, bool stencilStrokes, bool debug)
        {
            _antialias = edgeAA;
            _stencilStrokes = stencilStrokes;
            _debug = debug;
        }

    }
}