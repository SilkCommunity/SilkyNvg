using System.Collections.Generic;
using System.Numerics;
using SilkyNvg.Utils;

namespace SilkyNvg.Commands
{
    internal readonly struct MoveToCommand : IPathCommand
    {
    
        private readonly Vector2 _p;

        public MoveToCommand(Vector2 p)
        {
            _p = p;
        }

        public uint Build(List<Vector2> vertices, List<uint> vertexIndices, List<CommandType> commands, uint firstPathVertexIndex, RenderTolerances tolerances)
        {
            vertexIndices.Add((uint)vertices.Count);
            vertices.Add(_p);
            commands.Add(CommandType.MoveTo);
            return 1;
        }
    
    }
}
