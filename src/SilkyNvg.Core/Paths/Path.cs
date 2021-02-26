using SilkyNvg.Core.Geometry;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class Path
    {

        private readonly ICollection<Vertex> _fill = new List<Vertex>();
        private readonly ICollection<Vertex> _stroke = new List<Vertex>();

        private int _first;
        private int _bevelCount;

        private bool _winding;
        private bool _convex;

    }
}
