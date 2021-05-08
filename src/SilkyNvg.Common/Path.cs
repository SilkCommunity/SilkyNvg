using System.Collections.Generic;

namespace SilkyNvg.Common
{
    internal class Path
    {

        private readonly int _firstPoint;
        private readonly int _pointCount;

        private readonly List<Vertex> _fill = new();
        private readonly List<Vertex> _stroke = new();

        private Winding _winding;
        private bool _closed;
        private bool _convex;

    }
}
