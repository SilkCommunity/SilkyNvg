using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering
{
    public sealed class FrameContainer
    {
        
        private readonly DataAccessibleArrayList<Vector2> _points = new DataAccessibleArrayList<Vector2>();
        private readonly DataAccessibleArrayList<CommandData> _commands = new DataAccessibleArrayList<CommandData>();
        private readonly DataAccessibleArrayList<SubpathData> _subpaths = new DataAccessibleArrayList<SubpathData>();
        private readonly DataAccessibleArrayList<PathData> _paths = new DataAccessibleArrayList<PathData>();

        private readonly RenderTolerances _tolerances;
        
        public uint PointCount => (uint)_points.Count;
        
        public uint CommandCount => (uint)_commands.Count;
        
        public uint SubpathCount => (uint)_subpaths.Count;
        
        public uint PathCount => (uint)_paths.Count;
        
        public ReadOnlySpan<Vector2> Points => _points.ElementData;
        
        public ReadOnlySpan<CommandData> Commands => _commands.ElementData;
        
        public ReadOnlySpan<SubpathData> Subpaths => _subpaths.ElementData;
        
        public ReadOnlySpan<PathData> Paths => _paths.ElementData;

        public uint IntersectionsCount { get; private set; } = 0;

        internal Vector2 SubpathFirstPoint => _points.Count == 0 ? Vector2.Zero
            : _points[(int)_commands[(int)_spCommandIndex].PointIndex];
        
        // Current subpath cache
        private uint _spCommandIndex;
        private uint _spCommandNumber;
        private bool _spClosed;
        
        // Current path cache
        private uint _pSubpathIndex;
        private uint _pSubpathNumber;
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

        internal void AddCommand(uint nIntX, uint nIntY, CommandType type)
        {
            var commandData = new CommandData(
                pointIndex: PointCount,
                intersectionsTimeIndex: IntersectionsCount,
                type: type
            );
            _commands.Add(commandData);

            IntersectionsCount += nIntX + nIntY + 2; // Add 0 and 1 at start / end
            
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

        internal void MarkSubpathClosed()
        {
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

            IntersectionsCount = 0;
            
            _spCommandIndex = 0;
            _spCommandNumber = 0;
            _spClosed = false;

            _pSubpathIndex = 0;
            _pSubpathNumber = 0;
            _pFillRule = FillRule.NonZero;
        }
        
    }
}