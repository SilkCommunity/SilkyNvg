using SilkyNvg.Commands;

namespace SilkyNvg.Rendering
{
    public readonly struct CommandData
    {

        public readonly int PointIndex;
        public readonly CommandType Type;

        public CommandData(int pointIndex, CommandType type)
        {
            PointIndex = pointIndex;
            Type = type;
        }

    }
}