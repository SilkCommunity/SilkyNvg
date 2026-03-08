using System;
using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Commands;
using SilkyNvg.Utils;

namespace SilkyNvg.Rendering
{
    public sealed class FrameContainer
    {

        private readonly RenderTolerances _tolerances;

        // Vertices
        private readonly List<Vector2> _vertices = new List<Vector2>();
        
        // Curves
        private readonly List<CommandType> _pathCommands = new List<CommandType>();
        private readonly List<uint> _vertexIndices = new List<uint>();

        // Paths
        private readonly List<uint> _commandIndices = new List<uint>();
        private readonly List<uint> _commandCount = new List<uint>();

        // Public accessors for frame data
        public IReadOnlyList<Vector2> Vertices => _vertices;
        
        public IReadOnlyList<CommandType> PathCommands => _pathCommands;
        
        public IReadOnlyList<uint> VertexIndices => _vertexIndices;
        
        public IReadOnlyList<uint> CommandIndices => _commandIndices;
        
        public IReadOnlyList<uint> CommandCount => _commandCount;
        
        internal FrameContainer(RenderTolerances tolerances)
        {
            _tolerances = tolerances;
        }
        
        internal void AddPath(Path path)
        {
            uint startVertexIndex = (uint)_vertices.Count;
            uint startCommandIndex = (uint)_pathCommands.Count;
            uint pathCommandCount = 0;
            
            var commandEnumerator = path.GetCommandEnumerator();
            while (commandEnumerator.MoveNext())
            {
                var command = commandEnumerator.Current;
                if (command is null)
                {
                    throw new Exception("Path command enumeration failed.");
                }
                
                uint numberOfCommands = command.Build(_vertices, _vertexIndices, _pathCommands, startVertexIndex, _tolerances);
                pathCommandCount += numberOfCommands;
            }

            _commandIndices.Add(startCommandIndex);
            _commandCount.Add(pathCommandCount);
        }

        internal void Clear()
        {
            _vertices.Clear();
            
            _pathCommands.Clear();
            _vertexIndices.Clear();
            
            _commandIndices.Clear();
            _commandCount.Clear();
        }
        
    }
}
