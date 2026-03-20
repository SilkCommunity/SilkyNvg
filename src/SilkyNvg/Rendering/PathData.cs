namespace SilkyNvg.Rendering
{
    public readonly struct PathData
    {

        public readonly uint SubPathIndex;
        public readonly uint SubPathCount;

        internal PathData(uint subPathIndex, uint subPathCount)
        {
            SubPathIndex = subPathIndex;
            SubPathCount = subPathCount;
        }
        
    }
}