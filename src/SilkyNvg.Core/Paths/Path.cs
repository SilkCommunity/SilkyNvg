using SilkyNvg.API;
using SilkyNvg.Core.Geometry;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class Path
    {

        private readonly int _first;
        private readonly Winding _winding;

        private IList<Vertex> _fill = new List<Vertex>();
        private IList<Vertex> _stroke = new List<Vertex>();

        private int _count;
        private int _bevelCount;
        private bool _convex;
        private bool _closed;

        public List<Vertex> Fill
        {
            get => (List<Vertex>)_fill;
            set => _fill = value;
        }

        public List<Vertex> Stroke
        {
            get => (List<Vertex>)_stroke;
            set => _stroke = value;
        }

        public int First => _first;

        public Winding Winding => _winding;

        public int BevelCount
        {
            get => _bevelCount;
            set => _bevelCount = value;
        }

        public bool Convex
        {
            get => _convex;
            set => _convex = value;
        }

        public int Count
        {
            get => _count;
            set => _count = value;
        }

        public bool Closed => _closed;

        public Path(int first, Winding winding)
        {
            _first = first;
            _winding = winding;
        }

        public void Open()
        {
            _closed = false;
        }

        public void Close()
        {
            _closed = true;
        }

    }
}
