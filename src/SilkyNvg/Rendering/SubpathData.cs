using System.Runtime.InteropServices;

namespace SilkyNvg.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct SubpathData
    {

        internal readonly uint CommandIndex;
        internal readonly uint CommandNumber;
        internal readonly bool Closed;

        public SubpathData(uint commandIndex, uint commandNumber, bool closed)
        {
            CommandIndex = commandIndex;
            CommandNumber = commandNumber;
            Closed = closed;
        }

    }
}