using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering
{
    public sealed class FrameContainer
    {
        
        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<CommandData> _commands = new List<CommandData>();
        private readonly List<SubpathData> _subpaths = new List<SubpathData>();
        private readonly List<PathData> _paths = new List<PathData>();

        private readonly RenderTolerances _tolerances;
        
        public int PointCount => _points.Count;
        
        public int CommandCount => _commands.Count;
        
        public int SubpathCount => _subpaths.Count;
        
        public int PathCount => _paths.Count;
        
        // Current subpath cache
        private int _spCommandIndex;
        private int _spCommandNumber;
        private bool _spClosed;
        
        // Current path cache
        private int _pSubpathIndex;
        private int _pSubpathNumber;
        private FillRule _pFillRule;

        internal FrameContainer(RenderTolerances tolerances)
        {
            _tolerances = tolerances;
            
            _spCommandIndex = 0;
            _spCommandNumber = 0;
            _spClosed = false;

            _pSubpathIndex = 0;
            _pSubpathNumber = 0;
            _pFillRule = FillRule.NonZero;
        }
        
        internal void AddPoint(Vector2 p)
        {
            _points.Add(p);
        }

        internal void AddPoint(Vector2 p0, Vector2 p1)
        {
            _points.Add(p0);
            _points.Add(p1);
        }

        internal void AddPoint(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            _points.Add(p0);
            _points.Add(p1);
            _points.Add(p2);
        }

        internal void AddCommand(CommandType type)
        {
            var commandData = new CommandData(
                pointIndex: PointCount,
                type: type
            );
            _commands.Add(commandData);
            
            // increment number of commands in current subpath
            _spCommandNumber++;
        }

        internal void BeginSubpath(Vector2 point)
        {
            // Subpaths always start with a "moveTo" command.
            // In rendering, we can now assume there is always a point points[pointIndex - 1] in all commands processed.
            _points.Add(point);
            
            _spCommandIndex = CommandCount;
            _spCommandNumber = 0;
            _spClosed = false;

            // increment number of subpaths in current path
            _pSubpathNumber++;
        }

        internal void EndSubpath()
        {
            _subpaths.Add(new SubpathData(
                commandIndex: _spCommandIndex,
                commandNumber: _spCommandNumber,
                closed: _spClosed
            ));
        }

        internal void CloseSubpath()
        {
            Vector2 start = _points[_commands[_spCommandIndex].PointIndex - 1];
            Vector2 end = _points[_points.Count - 1];

            if (!start.FpEquals(end, _tolerances.FloatingPointTol))
            {
                AddCommand(CommandType.LineTo);
                AddPoint(start);
            }

            _spClosed = true;
        }
        
        internal void BeginPath()
        {
            _pSubpathIndex = SubpathCount;
            _pSubpathNumber = 0;
            _pFillRule = FillRule.NonZero;
        }

        internal void EndPath()
        {
            _paths.Add(new PathData(
                subpathIndex: _pSubpathIndex,
                subpathNumber: _pSubpathNumber,
                fillRule: _pFillRule
            ));
        }

        internal void Clear()
        {
            _points.Clear();
            _commands.Clear();
            _subpaths.Clear();
            _paths.Clear();
        }
        
    }
}