using System.Runtime.InteropServices;
using SilkyNvg.Commands;

namespace SilkyNvg.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CommandData
    {

        public readonly uint PointIndex;
        public readonly uint IntersectionsTimeIndex;
        public readonly uint NIntersectionsX;
        public readonly uint NIntersectionsY;
        public readonly CommandType Type;

        internal CommandData(uint pointIndex, uint intersectionsTimeIndex, uint nIntersectionsX, uint nIntersectionsY, CommandType type)
        {
            PointIndex = pointIndex;
            IntersectionsTimeIndex = intersectionsTimeIndex;
            NIntersectionsX = nIntersectionsX;
            NIntersectionsY = nIntersectionsY;
            Type = type;
        }

    }
}