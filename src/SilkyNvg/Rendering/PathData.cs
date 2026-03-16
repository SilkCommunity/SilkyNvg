using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PathData
    {

        public readonly uint SubpathIndex;
        public readonly uint SubpathNumber;

        public readonly FillRule FillRule;

        internal PathData(uint subpathIndex, uint subpathNumber, FillRule fillRule)
        {
            SubpathIndex = subpathIndex;
            SubpathNumber = subpathNumber;
            FillRule = fillRule;
        }

    }
}