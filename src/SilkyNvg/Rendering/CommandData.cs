using System.Runtime.InteropServices;
using SilkyNvg.Commands;

namespace SilkyNvg.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CommandData
    {

        public readonly uint PointIndex;
        public readonly uint IntersectionsTimeIndex;
        public readonly CommandType Type;

        internal CommandData(uint pointIndex, uint intersectionsTimeIndex, CommandType type)
        {
            PointIndex = pointIndex;
            IntersectionsTimeIndex = intersectionsTimeIndex;
            Type = type;
        }

    }
}