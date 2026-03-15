using SilkyNvg.Commands;

namespace SilkyNvg.Rendering
{
    public readonly struct CommandData
    {

        public readonly uint PointIndex;
        public readonly CommandType Type;

        public CommandData(uint pointIndex, CommandType type)
        {
            PointIndex = pointIndex;
            Type = type;
        }

    }
}