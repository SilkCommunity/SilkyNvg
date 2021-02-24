using SilkyNvg.Core.Geometry;
using System.Collections.Generic;

namespace SilkyNvg.Core.Paths
{
    public class Path
    {

        private List<Vertex> _fill = new List<Vertex>();
        private List<Vertex> _stroke = new List<Vertex>();

        private int _first;
        private int _count;
        private int _nbevel;
        private int _nfill;
        private int _nStroke;
        private Winding _winding;
        private bool _convex;
        private bool _closed;

        public List<Vertex> Fill => _fill;
        public List<Vertex> Stroke => _stroke;
        public int NFill
        {
            get => _nfill;
            set
            {
                _nfill = value;
            }
        }
        public int NStroke
        {
            get => _nStroke;
            set
            {
                _nStroke = value;
            }
        }
        public bool Convex
        {
            get => _convex;
            set
            {
                _convex = value;
            }
        }
        public bool Closed => _closed;
        public Winding Winding => _winding;
        public int First => _first;
        public int NBevel
        {
            get => _nbevel;
            set
            {
                _nbevel = value;
            }
        }

        public int Count
        {
            get => _count;
            set => _count = value;
        }

        public Path(int first, Winding winding)
        {
            _first = first;
            _winding = winding;
            _count = 0;
            _nbevel = 0;
            _nStroke = 0;
        }

        public void Close()
        {
            _closed = true;
        }

    }
}
