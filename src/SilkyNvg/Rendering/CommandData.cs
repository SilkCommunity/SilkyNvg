using System.Runtime.InteropServices;
using SilkyNvg.Commands;

namespace SilkyNvg.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
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