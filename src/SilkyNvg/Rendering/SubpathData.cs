namespace SilkyNvg.Rendering
{
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