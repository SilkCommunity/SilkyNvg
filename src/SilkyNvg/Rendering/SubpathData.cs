namespace SilkyNvg.Rendering
{
    public readonly struct SubpathData
    {

        internal readonly int CommandIndex;
        internal readonly int CommandNumber;
        internal readonly bool Closed;

        public SubpathData(int commandIndex, int commandNumber, bool closed)
        {
            CommandIndex = commandIndex;
            CommandNumber = commandNumber;
            Closed = closed;
        }

    }
}