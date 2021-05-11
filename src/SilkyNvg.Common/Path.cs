using System.Collections.Generic;

namespace SilkyNvg.Common
{
    internal class Path
    {

        private readonly int _firstPoint;

        private readonly List<Vertex> _fill = new();
        private readonly List<Vertex> _stroke = new();

        public bool Convex { get; set; }

        public Winding Winding { get; set; }

        public bool Closed { get; set; }

        public int BevelCount { get; set; }

        public int PointCount { get; set; }

        public int First => _firstPoint;

        public Path(int pointCount)
        {
            _firstPoint = pointCount;
            Winding = Winding.CCW;
        }

    }
}
